using Host.Infrastructure;
using Host.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Host.Configuration;

/// <summary>
/// Настраивает OpenIdConnectOptions BFF из <see cref="OidcOptions"/>:
/// endpoint-ы и учётные данные клиента, Auth Code + PKCE, сохранение токенов
/// и события входа (идентификатор BFF-сессии, режим «запомнить меня», единое expires_at).
/// </summary>
public sealed class ConfigureOidcOptions(IOptions<OidcOptions> oidc, IOptions<SpaOptions> spa, TimeProvider timeProvider)
    : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly OidcOptions _oidc = oidc.Value;
    private readonly SpaOptions _spa = spa.Value;

    /// <summary>Применяет настройку только к именованной схеме OpenIdConnect; другие имена игнорирует.</summary>
    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (name != OpenIdConnectDefaults.AuthenticationScheme)
            return;

        Configure(options);
    }

    /// <summary>Применяет настройку к заданному экземпляру options.</summary>
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

        // Срабатывает после проверки id_token, то есть только для входа, который точно состоится.
        // Здесь же ещё доступен сырой ответ token-endpoint (refresh_expires_in в SaveTokens не попадает).
        options.Events ??= new OpenIdConnectEvents();

        // fetch из SPA не может проследовать за 302 в Keycloak: для /api/** вместо редиректа
        // отвечаем 401 problem+json. Редирект остаётся для браузерных переходов (/login и пр.).
        options.Events.OnRedirectToIdentityProvider = async ctx =>
        {
            if (!ctx.Request.Path.StartsWithSegments("/api"))
                return;

            await ApiProblem.WriteUnauthorizedAsync(ctx.Response, ApiProblem.NotAuthenticated);
            ctx.HandleResponse();
        };
        options.Events.OnTokenValidated = ctx =>
        {
            var properties = ctx.Properties!;

            // Стабильный идентификатор BFF-сессии: ставится один раз, переживает refresh.
            // Ключ сериализации обновления токенов (session-over-session, ADR 0027).
            properties.SetString(BffSessionKeys.SessionId, Guid.NewGuid().ToString("N"));

            // Режим «запомнить меня»: Keycloak не кладёт его в токен, но refresh-токен живёт по SSO Idle режима (~30 мин против ~30 дней).
            // TokenEndpointResponse: {access_token, id_token, refresh_token, expires_in, refresh_expires_in, ...}.
            string? raw = null;
            ctx.TokenEndpointResponse?.Parameters.TryGetValue("refresh_expires_in", out raw);
            var refreshTokenLifetime = long.TryParse(raw, out var seconds)
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.Zero;
            var isRememberMe = refreshTokenLifetime > _oidc.TokenRefresh.RememberMeThreshold;

            // Персистентность и окно cookie по режиму (ADR 0027).
            // Дальше окно сдвигает только token-refresh (ResetWindow): встроенное скольжение выключено.
            SessionCookieLifetime.Apply(properties, isRememberMe, _oidc.SsoSession, timeProvider.GetUtcNow());

            return Task.CompletedTask;
        };

        // Срабатывает после SaveTokens (в OnTokenValidated токенов в properties ещё нет)
        // и до SignIn тикета в cookie и стор.
        options.Events.OnTicketReceived = ctx =>
        {
            // SaveTokens пишет expires_at = now + expires_in. Приводим к правилу refresh
            // (минус ClockSkewBuffer), иначе первый refresh после входа наступает позже последующих.
            var properties = ctx.Properties!;
            if (AccessTokenExpiryHelper.TryParse(properties.GetTokenValue(AccessTokenExpiryHelper.TokenName), out var expiresAt))
            {
                properties.UpdateTokenValue(
                    AccessTokenExpiryHelper.TokenName,
                    AccessTokenExpiryHelper.Format(
                        AccessTokenExpiryHelper.Effective(expiresAt, _oidc.TokenRefresh.ClockSkewBuffer)
                    )
                );
            }

            return Task.CompletedTask;
        };

        // Обработка отказа Keycloak (invalid redirect_uri, недоступный OP) и отказа пользователя (access_denied).
        // Без этих обработчиков ошибка выливается 500 на /signin-oidc - адресе, где пользователь
        // уже не может ничего сделать. Уводим в SPA с кодом, детали остаются в логе.
        options.Events.OnRemoteFailure = ctx =>
        {
            LogAuthFailure(ctx.HttpContext, ctx.Failure, "remote failure");
            RedirectToSpa(ctx.Response, "remote_failure");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };
        options.Events.OnAccessDenied = ctx =>
        {
            LogAuthFailure(ctx.HttpContext, null, "access denied");
            RedirectToSpa(ctx.Response, "access_denied");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };
    }

    private static void LogAuthFailure(HttpContext context, Exception? failure, string reason) =>
        context
            .RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Host.Oidc")
            .LogError(failure, "OIDC sign-in failed: {Reason}", reason);

    /// <summary>
    /// Перенаправляет пользователя на SPA с кодом ошибки в параметре.
    /// </summary>
    /// <param name="response">Текущий запрос <see cref="HttpResponse"/></param>
    /// <param name="errorCode">Понятный код ошибки: access_denied, remote_failure, ...</param>
    private void RedirectToSpa(HttpResponse response, string errorCode) =>
        response.Redirect($"{_spa.BaseUrl}/?authError={errorCode}");
}
