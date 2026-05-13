using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FinanceTrack.Gateway.Configuration;

/// <summary>
/// Configures OpenIdConnect options using OidcOptions from configuration
/// </summary>
public class ConfigureOidcOptions(IOptions<OidcOptions> oidcOptions, IHostEnvironment env)
    : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (name != OpenIdConnectDefaults.AuthenticationScheme)
            return;

        Configure(options);
    }

    public void Configure(OpenIdConnectOptions options)
    {
        options.Authority = _oidcOptions.Bff.Authority;
        options.ClientId = _oidcOptions.Bff.ClientId;
        options.ClientSecret = _oidcOptions.Bff.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles",
        };

        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.SignedOutRedirectUri = "/";

        if (env.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
            // http bypass section (dev only)
            options.NonceCookie.SameSite = SameSiteMode.Unspecified;
            options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        }
    }
}
