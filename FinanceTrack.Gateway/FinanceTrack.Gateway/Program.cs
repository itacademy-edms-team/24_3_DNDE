using FinanceTrack.Gateway.Configuration;
using FinanceTrack.Gateway.Extensions;
using FinanceTrack.Gateway.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder
    .Services.AddOptions<OidcOptions>()
    .Bind(builder.Configuration.GetSection(OidcOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Services
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ITokenExchangeService, TokenExchangeService>();

// YARP Reverse Proxy with Token Exchange
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTokenExchangeTransform();

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

app.UseAuthentication();
app.UseAuthorization();
app.UseKeycloakTokenRefresh();

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
    )
    .RequireAuthorization();

await app.RunAsync();
