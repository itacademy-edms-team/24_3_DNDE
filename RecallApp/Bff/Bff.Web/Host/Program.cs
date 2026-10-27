using Host.Configuration;
using Host.Extensions;
using Host.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Remove default Microsoft logging providers
builder.Logging.ClearProviders();
// Register NLog
builder.Host.UseNLog();
var bootstrapLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    bootstrapLogger.Info("Starting Web Host...");

    // Configuration
    builder.Services.AddOptions<OidcOptions>()
        .Bind(builder.Configuration.GetSection(OidcOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

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

    // Серверное хранилище сессии (ADR 0021): in-memory за IDistributedCache, Redis по триггеру.
    builder.Services.AddDistributedMemoryCache();
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

            // Окно сессии задаётся на входе per-mode (SessionCookieLifetime.Apply: IssuedUtc + ExpiresUtc
            // = SSO Idle режима, ADR 0027). Встроенное скольжение продлевает с сохранением длины окна
            // (ExpiresUtc - IssuedUtc), поэтому 30 минут / 30 дней держатся сами.
            options.SlidingExpiration = true;
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

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseKeycloakTokenRefresh();
    app.UseAuthorization();

    app.MapHealthChecks("/healthz");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    bootstrapLogger.Error(ex, "Application startup failed");
}
finally
{
    LogManager.Shutdown();
}
