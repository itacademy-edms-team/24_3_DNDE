using Host.Configuration;
using Host.Extensions;
using Host.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using NLog.Web;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Remove default Microsoft logging providers
builder.Logging.ClearProviders();
// Register NLog (nlog.config)
builder.Host.UseNLog();
var bootstrapLogger = LogManager.GetCurrentClassLogger();

try
{
    bootstrapLogger.Info("Starting Web Host...");

    // Configuration
    builder.Services.AddOptions<OidcOptions>()
        .Bind(builder.Configuration.GetSection(OidcOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<OidcOptions>, OidcOptionsValidator>();

    builder.Services.AddOptions<SpaOptions>()
        .Bind(builder.Configuration.GetSection(SpaOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<SpaOptions>, SpaOptionsValidator>();

    builder.Services.AddOptions<DiagnosticsOptions>()
        .Bind(builder.Configuration.GetSection(DiagnosticsOptions.SectionName));

    // Right scheme/host/port forward (за edge-прокси, ADR 0026)
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor
            | ForwardedHeaders.XForwardedProto
            | ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // Services
    builder.Services.AddHealthChecks();

    builder.Services.AddSingleton(TimeProvider.System);

    // Серверное хранилище сессии (ADR 0021): in-memory за IDistributedCache, Redis по триггеру.
    // Лимит памяти задаём явно, берём дефолт из фреймворка.
    builder.Services.AddDistributedMemoryCache();
    builder.Services
        .AddOptions<MemoryDistributedCacheOptions>()
        .Configure<IOptions<OidcOptions>>(
            (cacheOptions, oidc) =>
                cacheOptions.SizeLimit = oidc.Value.SessionStoreSizeLimitBytes
        );
    builder.Services.AddSingleton<ITicketStore, DistributedCacheTicketStore>();

    // BFF-аутентификация: cookie-сессия + OIDC-вход в Keycloak (ADR 0002).
    builder.Services.ConfigureOptions<ConfigureOidcOptions>();
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "recall_bff";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;

            // Окно сессии задаётся на входе per-mode (SessionCookieLifetime.Apply: ExpiresUtc
            // = SSO Idle режима, ADR 0027) и сдвигается только на token refresh (ResetWindow).
            // Встроенное скольжение выключено: оно сохраняет в стор копию тикета, прочитанную
            // на входе запроса. Параллельные запросы после простоя перезаписывали так новые
            // токены старыми, отозванными (сценарий 9 ручных тестов). Кроме того, Keycloak
            // сбрасывает SSO Idle только на refresh, и окно cookie теперь сдвигается тем же событием.
            options.SlidingExpiration = false;
        })
        .AddOpenIdConnect();

    builder.Services
        .AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
        .Configure<ITicketStore, IOptions<OidcOptions>>(
            (options, store, oidc) =>
            {
                options.SessionStore = store;
                // Запасной срок, если на входе ExpiresUtc не передан: короткий обычный SSO Idle,
                // а не дефолтные 14 дней.
                options.ExpireTimeSpan = oidc.Value.SsoSession.SessionIdle;
            }
        );

    builder.Services.AddKeycloakTokenRefreshServices();

    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
            // Добавить bff, как источник в ответе
            ctx.ProblemDetails.Extensions["source"] = ProblemSource.Bff
    );

    builder.Services.AddControllers();

    builder.Services.AddCorsUsingOptions(builder.Configuration);

    // YARP: проксирование /api/** -> монолит (ADR 0028, Trusted Domain).
    // Access-токен текущей сессии прикрепляется трансформером;
    // клиентские Authorization/Cookie отбрасываются - монолит доверяет только тому, что решил BFF.
    // Включается только при заданном AppProxy:Destination (compose).
    var appProxyDestination = builder.Configuration["AppProxy:Destination"];
    if (appProxyDestination is not null)
    {
        builder.Services
            .AddReverseProxy()
            .LoadFromMemory(
                new[]
                {
                    new RouteConfig
                    {
                        RouteId = "app-api",
                        ClusterId = "app",
                        Match = new RouteMatch { Path = "/api/{**catch-all}" },
                    },
                },
                new[]
                {
                    new ClusterConfig
                    {
                        ClusterId = "app",
                        Destinations = new Dictionary<string, DestinationConfig>
                        {
                            ["app"] = new DestinationConfig { Address = appProxyDestination },
                        },
                    },
                })
            .AddTransforms(transforms => transforms.AddRequestTransform(async context =>
            {
                context.ProxyRequest.Headers.Authorization = null;
                context.ProxyRequest.Headers.Remove("Cookie");

                var auth = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var token = auth.Properties?.GetTokenValue("access_token");
                if (token is not null)
                    context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }));
    }

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
        app.UseExceptionHandler();
    app.UseForwardedHeaders();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseCorsUsingOptions();
    app.UseAuthentication();
    app.UseKeycloakTokenRefresh();
    app.UseSessionGuard();
    app.UseAuthorization();

    app.MapHealthChecks("/healthz");
    app.MapControllers();
    if (appProxyDestination is not null)
        app.MapReverseProxy();

    app.Run();
}
// HostAbortedException бросает WebApplicationFactory, когда забирает собранный хост в тестах.
catch (Exception ex) when (ex is not HostAbortedException)
{
    bootstrapLogger.Error(ex, "Application startup failed");
}
finally
{
    LogManager.Shutdown();
}

public partial class Program;
