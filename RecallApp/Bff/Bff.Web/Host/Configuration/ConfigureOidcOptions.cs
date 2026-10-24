using Host.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Host.Configuration;

/// <summary>
/// Настраивает OpenIdConnectOptions из <see cref="OidcOptions"/>:
/// </summary>
public sealed class ConfigureOidcOptions(IOptions<OidcOptions> oidc)
    : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly OidcOptions _oidc = oidc.Value;

    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (name != OpenIdConnectDefaults.AuthenticationScheme)
            return;

        Configure(options);
    }

    public void Configure(OpenIdConnectOptions options)
    {
        options.Authority = _oidc.Authority;
        options.ClientId = _oidc.ClientId;
        options.ClientSecret = _oidc.ClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;

        // Токены хранятся в серверном тикете (ADR 0021), а не отдаются клиенту.
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        // Не переименовывать входящие claim-типы; нужен исходный sub, resource_access и т.д.
        options.MapInboundClaims = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.SignedOutRedirectUri = "/";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            // Клиентские роли RecallApp лежат в resource_access (ADR 0024);
            // плоского claim "roles" без маппера Keycloak нет - маппинг ролей в Этапе C.
            RoleClaimType = "roles",
        };

        // Стабильный идентификатор BFF-сессии ставится один раз при входе и переживает refresh.
        // Ключ сериализации обновления токенов (session-over-session, ADR 0027).
        options.Events ??= new OpenIdConnectEvents();
        options.Events.OnTokenValidated = ctx =>
        {
            ctx.Properties?.SetString(BffSessionKeys.SessionId, Guid.NewGuid().ToString("N"));
            return Task.CompletedTask;
        };
    }
}
