using Host.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Host.Services;

public interface ITokenRefreshService
{
    Task TryRefreshTokenAsync(HttpContext context);
}

/// <summary>
/// Координатор обновления access-токена: решает, нужно ли обновлять, и сериализует обновление на BFF-сессию.
/// Запрос к Keycloak делает <see cref="KeycloakTokenClient"/>, запись в сессию - <see cref="SessionTokenWriter"/>.
/// Отказ Keycloak (400/401) делает sign-out серверной сессии;
/// временный сбой (5xx, таймаут, сеть, битый ответ) сессию сохраняет и откладывает повтор на <see cref="TokenLifetimeOptions.RefreshRetryBackoff"/>.
/// </summary>
public sealed class TokenRefreshService(
    KeycloakTokenClient tokenClient,
    SessionTokenWriter sessionWriter,
    IOptions<OidcOptions> oidcOptions,
    TimeProvider timeProvider,
    ILogger<TokenRefreshService> logger,
    TokenRefreshSemaphores semaphores
) : ITokenRefreshService
{
    /// <summary>Мягкий потолок числа записей замков, за которым включается ленивый trim.</summary>
    private const int SessionLockSoftCap = 10_000;

    /// <summary>Сколько старых записей снимает один ленивый trim.</summary>
    private const int LazyTrimBatch = 100;

    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    public async Task TryRefreshTokenAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        // Одна аутентификация на запрос (результат кэшируется): отсюда берём и properties
        // с токенами, и идентификатор BFF-сессии для ключа сериализации.
        var authResult = await context
            .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            .ConfigureAwait(false);
        if (!authResult.Succeeded || authResult.Properties is null)
            return;

        // MapInboundClaims = false: claim sub приходит как есть, без переименования в NameIdentifier.
        var sub = context.User.FindFirst("sub")?.Value ?? "unknown";

        var sessionKey = authResult.Properties.GetString(BffSessionKeys.SessionId);
        if (string.IsNullOrEmpty(sessionKey))
        {
            // Cookie без BFF-session-id (выписана до появления маппера). Деградируем на sub,
            // чтобы не падать; корректность мультисессии при этом не гарантируется.
            logger.LogWarning(
                "Token refresh: BFF session id missing, falling back to sub for sub={Sub}",
                sub
            );
            sessionKey = sub;
        }

        var expiresAtRaw = authResult.Properties.GetTokenValue(AccessTokenExpiryHelper.TokenName);
        if (string.IsNullOrEmpty(expiresAtRaw))
        {
            logger.LogWarning(
                "Token refresh skipped: expires_at not found in session {SessionKey} (sub={Sub})",
                sessionKey,
                sub
            );
            return;
        }

        if (!AccessTokenExpiryHelper.TryParse(expiresAtRaw, out var expiresAt))
        {
            logger.LogWarning(
                "Token refresh skipped: failed to parse expires_at='{ExpiresAt}' (sub={Sub})",
                expiresAtRaw,
                sub
            );
            return;
        }

        var refreshThreshold = _oidcOptions.TokenLifetime.RefreshThreshold;
        var timeUntilExpiry = expiresAt - timeProvider.GetUtcNow();
        logger.LogDebug(
            "Token refresh check: timeUntilExpiry={TimeUntilExpiry:g}, threshold={Threshold:g}, session={SessionKey}",
            timeUntilExpiry,
            refreshThreshold,
            sessionKey
        );

        if (timeUntilExpiry > refreshThreshold)
            return;

        // Пауза после временного сбоя: не встаём в очередь на замок, чтобы не ждать таймаут Keycloak.
        if (IsInRetryBackoff(sessionKey))
            return;

        var sem = semaphores.Semaphores.GetOrAdd(sessionKey, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync().ConfigureAwait(false);
        try
        {
            if (
                semaphores.LastRefreshedAt.TryGetValue(sessionKey, out var lastRefreshed)
                && timeProvider.GetUtcNow() - lastRefreshed < refreshThreshold
            )
            {
                logger.LogDebug(
                    "Token refresh skipped (double-check): session={SessionKey}",
                    sessionKey
                );
                return;
            }

            // Повторная проверка паузы: запросы, ждавшие замок во время неудачной попытки,
            // не должны сразу повторять её.
            if (IsInRetryBackoff(sessionKey))
                return;

            logger.LogInformation(
                "Performing token refresh for session={SessionKey} (sub={Sub})",
                sessionKey,
                sub
            );

            var refreshToken = authResult.Properties.GetTokenValue("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning(
                    "Token refresh failed: refresh_token missing for session={SessionKey}",
                    sessionKey
                );
                await SignOutAsync(context).ConfigureAwait(false);
                return;
            }

            var result = await tokenClient.RefreshAsync(refreshToken).ConfigureAwait(false);
            switch (result.Outcome)
            {
                case TokenRefreshOutcome.Success:
                    await sessionWriter
                        .StoreRefreshedTokensAsync(context, authResult, result.Tokens!)
                        .ConfigureAwait(false);

                    semaphores.LastRefreshedAt[sessionKey] = timeProvider.GetUtcNow();
                    semaphores.RetryNotBefore.TryRemove(sessionKey, out _);
                    logger.LogInformation(
                        "Token refresh succeeded for session={SessionKey}",
                        sessionKey
                    );

                    // Ленивый guard: страховка от всплеска логинов между проходами уборщика.
                    if (semaphores.Semaphores.Count > SessionLockSoftCap)
                        semaphores.TrimOldest(LazyTrimBatch);
                    break;

                case TokenRefreshOutcome.Unavailable:
                    // Сессию не трогаем и LastRefreshedAt не ставим: после паузы попытка повторится.
                    var retryAt =
                        timeProvider.GetUtcNow() + _oidcOptions.TokenLifetime.RefreshRetryBackoff;
                    semaphores.RetryNotBefore[sessionKey] = retryAt;
                    logger.LogWarning(
                        "Token refresh deferred: IdP unavailable, session kept, retry after {RetryAt:o} for session={SessionKey}",
                        retryAt,
                        sessionKey
                    );
                    break;

                case TokenRefreshOutcome.Rejected:
                    await SignOutAsync(context).ConfigureAwait(false);
                    break;
            }
        }
        finally
        {
            sem.Release();
        }
    }

    private bool IsInRetryBackoff(string sessionKey)
    {
        if (
            !semaphores.RetryNotBefore.TryGetValue(sessionKey, out var retryNotBefore)
            || timeProvider.GetUtcNow() >= retryNotBefore
        )
            return false;

        logger.LogDebug(
            "Token refresh skipped (retry backoff until {RetryNotBefore:o}): session={SessionKey}",
            retryNotBefore,
            sessionKey
        );
        return true;
    }

    private static Task SignOutAsync(HttpContext context) =>
        context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
