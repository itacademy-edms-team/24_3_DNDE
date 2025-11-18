using FinanceTrack.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Options
// Oidc options
builder
    .Services.AddOptions<OidcOptions>()
    .Bind(builder.Configuration.GetSection(OidcOptions.SectionName));

// Yarp
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Bff
var oidcOptions = builder.Configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>();
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
            options.Cookie.SameSite = SameSiteMode.Lax; // dev, SPA on same origin
            // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // prod
        }
    )
    .AddOpenIdConnect(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            // Keycloak directly, no proxy, (server to server)
            options.Authority = oidcOptions.Authority;
            options.RequireHttpsMetadata = false; // dev only

            options.ClientId = oidcOptions.ClientId; // ClientId from keycloak
            options.ClientSecret = oidcOptions.ClientSecret; // ClientSecret from keycloak
            options.ResponseType = OpenIdConnectResponseType.Code;

            options.SaveTokens = true; // access/refresh tokens saved in auth-sessions
            options.GetClaimsFromUserInfoEndpoint = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "preferred_username",
                RoleClaimType = "roles",
            };

            // callback URL's must match with Keycloak client settings
            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-callback-oidc";
            options.SignedOutRedirectUri = "/";
        }
    );

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

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
            if (!ctx.User?.Identity?.IsAuthenticated ?? true)
                return Results.Unauthorized();

            var name = ctx.User.Identity!.Name;
            var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });

            return Results.Ok(new { name, claims });
        }
    )
    .RequireAuthorization();

// Example of a protected API endpoint
//app.MapGet("/api/transactions", () => ...)
//   .RequireAuthorization();

await app.RunAsync();
