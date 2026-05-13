using FinanceTrack.Gateway.Configuration;
using FinanceTrack.Gateway.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().CreateLogger();
builder.Host.UseSerilog((_, config) => config.ReadFrom.Configuration(builder.Configuration));

logger.Information("Starting Web Host ...");

// Configuration
builder
    .Services.AddOptions<OidcOptions>()
    .Bind(builder.Configuration.GetSection(OidcOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Services
builder.Services.AddHealthChecks();
builder.Services.AddKeycloakTokenRefreshServices();
builder.Services.AddKeycloakTokenExchangeServices();

// YARP Reverse Proxy — ITransformProvider is auto-discovered from DI
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient(
        (_, handler) =>
        {
            handler.PooledConnectionLifetime = TimeSpan.FromSeconds(30);
            handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(20);
        }
    );

// Right scheme/host/port under Traefik
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// BFF Authentication
builder.Services.ConfigureOptions<ConfigureOidcOptions>();
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.Cookie.Name = "ft_bff_auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        }
    )
    .AddOpenIdConnect();

var app = builder.Build();

// UseRouting объявляем явно, чтобы у middleware токен-рефреша и токен-обмена был доступ к Endpoint и его метаданным.
app.UseRouting();
app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.UseKeycloakTokenRefresh();
app.UseKeycloakTokenExchange();

app.MapHealthChecks("/healthz");
app.MapReverseProxy();

// BFF endpoints
app.MapGet(
    "/bff/login",
    async context =>
    {
        await context.ChallengeAsync(
            OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = "/" }
        );
    }
);

app.MapGet(
    "/bff/logout",
    async context =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
);

app.MapGet(
    "/bff/user",
    (HttpContext ctx) =>
    {
        if (ctx.User.Identity is not { IsAuthenticated: true } identity)
            return Results.Unauthorized();

        var name = identity.Name;
        var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });

        return Results.Ok(new { name, claims });
    }
);

await app.RunAsync();
