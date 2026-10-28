using System.Globalization;
using System.Security.Claims;
using Host.Configuration;
using Host.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Host.Services;

/// <summary>
/// Записывает токены в BFF-сессию (cookie + серверный стор).
/// Единственная точка ручного SignIn на cookie-схеме после входа: сохранение сессии
/// всегда сдвигает окно (<see cref="SessionCookieLifetime.ResetWindow"/>), ADR 0027.
/// </summary>
public sealed class SessionTokenWriter(IOptions<OidcOptions> oidcOptions, TimeProvider timeProvider)
{
    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    /// <summary>
    /// Записать токены после успешного refresh в сессию текущего запроса и сохранить её.
    /// <paramref name="session"/> - успешный результат AuthenticateAsync(Cookies) этого запроса.
    /// Его Properties запомнены handler-ом на весь запрос, поэтому GetTokenAsync ниже по конвейеру
    /// в этом же запросе уже отдаёт новые токены.
    /// </summary>
    public async Task StoreRefreshedTokensAsync(
        HttpContext context,
        AuthenticateResult session,
        RefreshedTokens tokens
    )
    {
        var properties = session.Properties!;
        var now = timeProvider.GetUtcNow();

        var expiresAt = now.Add(tokens.ExpiresIn - _oidcOptions.TokenLifetime.ClockSkewBuffer)
            .ToString("o", CultureInfo.InvariantCulture);

        properties.StoreTokens(
            [
                new() { Name = "access_token", Value = tokens.AccessToken },
                new()
                {
                    Name = "refresh_token",
                    Value = tokens.RefreshToken ?? properties.GetTokenValue("refresh_token")!,
                },
                new()
                {
                    Name = "id_token",
                    Value = tokens.IdToken ?? properties.GetTokenValue("id_token")!,
                },
                new() { Name = "expires_at", Value = expiresAt },
            ]
        );

        await SaveSessionAsync(context, session.Principal!, properties, now).ConfigureAwait(false);
    }

    /// <summary>
    /// Сохранить сессию: начать окно заново и выполнить SignIn (cookie + RenewAsync стора тем же ключом).
    /// Встроенное скольжение cookie выключено, поэтому окно сдвигается только здесь:
    /// без ResetWindow в cookie и стор ушли бы старые даты из переданных properties.
    /// </summary>
    private Task SaveSessionAsync(
        HttpContext context,
        ClaimsPrincipal principal,
        AuthenticationProperties properties,
        DateTimeOffset now
    )
    {
        // IssuedUtc = now, ExpiresUtc = now + SSO Idle режима (режим по маркеру bff_remember_me).
        SessionCookieLifetime.ResetWindow(properties, _oidcOptions.SsoSession, now);
        return context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties
        );
    }
}
