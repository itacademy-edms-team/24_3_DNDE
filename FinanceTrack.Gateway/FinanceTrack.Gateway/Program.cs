using FinanceTrack.Gateway.Configuration;
using FinanceTrack.Gateway.Extensions;
using FinanceTrack.Gateway.Services;
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
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TokenExchangeLocks>();
builder
    .Services.AddHttpClient<ITokenExchangeService, TokenExchangeService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(4),
        }
    );
builder
    .Services.AddHttpClient(
        KeycloakTokenRefreshMiddleware.HttpClientName,
        client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        }
    )
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(4),
        }
    );
builder.Services.AddScoped<KeycloakTokenRefreshMiddleware>();

// YARP Reverse Proxy with Token Exchange
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTokenExchangeTransform()
    .ConfigureHttpClient((_, handler) =>
    {
        handler.PooledConnectionLifetime = TimeSpan.FromSeconds(30);
        handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(20);
    });

//// Right scheme/host/port indent under Traefik
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

app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<KeycloakTokenRefreshMiddleware>();

app.MapHealthChecks("/healthz");

// Yarp. Proxy all routes in configuration
app.MapReverseProxy();

// BFF endpoints
app.MapGet(
    "/bff/login",
    async context =>
    {
        // Trigger external login: redirect to Keycloak
        await context.ChallengeAsync(
            OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = "/", // Where to return after login
            }
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
