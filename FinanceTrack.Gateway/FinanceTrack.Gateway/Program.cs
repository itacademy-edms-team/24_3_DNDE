using System.Net.Http.Headers;
using FinanceTrack.Gateway.Configuration;
using FinanceTrack.Gateway.Extensions;
using FinanceTrack.Gateway.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Options
// Oidc options
builder
    .Services.AddOptions<OidcOptions>()
    .Bind(builder.Configuration.GetSection(OidcOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Memory cache for token exchange
builder.Services.AddMemoryCache();

// Token Exchange Service
builder.Services.AddHttpClient<ITokenExchangeService, TokenExchangeService>();

// Yarp
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        // Apply token exchange transform to routes that have configured services
        var routeId = builderContext.Route.RouteId;
        
        builderContext.AddRequestTransform(async transformContext =>
        {
            var httpContext = transformContext.HttpContext;
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            // Get the original access token
            var accessToken = httpContext.Items.TryGetValue(
                "bff_access_token",
                out var tokenObj
            )
                ? tokenObj as string
                : await httpContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                // No token - might be a public route
                return;
            }

            // Check if token exchange is enabled and configured for this route
            var oidcOptions = httpContext.RequestServices
                .GetRequiredService<IOptions<OidcOptions>>().Value;

            var serviceConfig = oidcOptions.TokenExchange.GetByRouteId(routeId);
            
            if (oidcOptions.TokenExchange.Enabled && 
                serviceConfig != null && 
                !string.IsNullOrEmpty(serviceConfig.Audience))
            {
                // Perform token exchange
                var tokenExchangeService = httpContext.RequestServices
                    .GetRequiredService<ITokenExchangeService>();

                var exchangeResult = await tokenExchangeService.ExchangeTokenAsync(
                    accessToken,
                    serviceConfig.Audience,
                    serviceConfig.Scopes,
                    httpContext.RequestAborted
                );

                if (exchangeResult.Success && !string.IsNullOrEmpty(exchangeResult.AccessToken))
                {
                    accessToken = exchangeResult.AccessToken;
                    logger.LogDebug(
                        "Using exchanged token for route {RouteId} (audience: {Audience})",
                        routeId,
                        serviceConfig.Audience
                    );
                }
                else
                {
                    logger.LogWarning(
                        "Token exchange failed for route {RouteId}: {Error} - {Description}. Using original token.",
                        routeId,
                        exchangeResult.Error,
                        exchangeResult.ErrorDescription
                    );
                }
            }

            transformContext.ProxyRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        });
    });

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
            // Refresh-token lifetime
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        }
    )
    .AddOpenIdConnect(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            // Keycloak directly, no proxy, (server to server)
            options.Authority = oidcOptions.Bff.Authority;
            options.RequireHttpsMetadata = false; // dev only

            options.ClientId = oidcOptions.Bff.ClientId; // ClientId from keycloak
            options.ClientSecret = oidcOptions.Bff.ClientSecret; // ClientSecret from keycloak
            options.ResponseType = OpenIdConnectResponseType.Code;

            options.SaveTokens = true; // access/refresh tokens saved in auth-sessions (cookie)
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
            if (!ctx.User?.Identity?.IsAuthenticated ?? true)
                return Results.Unauthorized();

            var name = ctx.User.Identity!.Name;
            var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });

            return Results.Ok(new { name, claims });
        }
    )
    .RequireAuthorization();

app.MapGet(
    "/debug/tokens",
    async (HttpContext ctx) =>
    {
        return Results.Json(
            new
            {
                access = await ctx.GetTokenAsync("access_token"),
                refresh = await ctx.GetTokenAsync("refresh_token"),
                id = await ctx.GetTokenAsync("id_token"),
            }
        );
    }
);

// Example of a protected API endpoint
//app.MapGet("/api/transactions", () => ...)
//   .RequireAuthorization();

await app.RunAsync();
