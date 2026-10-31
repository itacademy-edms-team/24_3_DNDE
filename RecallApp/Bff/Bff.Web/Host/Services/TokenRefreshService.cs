using Host.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Host.Services;

/// <summary>
/// Координатор обновления access-токена: решает, нужно ли обновлять, и сериализует обновление на BFF-сессию.
/// Запрос к Keycloak делает <see cref="KeycloakTokenClient"/>, запись в сессию - <see cref="SessionTokenWriter"/>.
/// Отказ Keycloak (400/401) делает sign-out серверной сессии;
/// временный сбой (5xx, таймаут, сеть, битый ответ) сессию сохраняет и откладывает повтор на <see cref="TokenRefreshOptions.RefreshRetryBackoff"/>.
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

    /// <inheritdoc/>
    public async Task RefreshIfNeededAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        // Одна аутентификация на запрос (результат кэшируется): отсюда берём и properties
        // с токенами, и идентификатор BFF-сессии для ключа сериализации.
        var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Properties is null)
            return;

        // MapInboundClaims = false: claim sub приходит как есть, без переименования в NameIdentifier.
        var sub = context.User.FindFirst("sub")?.Value ?? "unknown";

        var sessionKey = authResult.Properties.GetString(BffSessionKeys.SessionId);
        if (string.IsNullOrEmpty(sessionKey))
        {
            // Ключ сериализации обязателен: сессия без него (cookie выписана до появления маппера)
            // не сможет корректно обновляться - завершаем её, в Keycloak не ходим.
            logger.LogWarning(
                "Token refresh: BFF session id missing, signing out session for sub={Sub}",
                sub
            );
            MarkSessionRejected(context);
            await SignOutAsync(context);
            return;
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

        var refreshThreshold = _oidcOptions.TokenRefresh.RefreshThreshold;
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

        var semaphore = semaphores.Semaphores.GetOrAdd(sessionKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(context.RequestAborted);
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
                MarkSessionRejected(context);
                await SignOutAsync(context);
                return;
            }

            var result = await tokenClient.RefreshAsync(refreshToken, context.RequestAborted);
            switch (result.Outcome)
            {
                case TokenRefreshOutcome.Success:
                    await sessionWriter.StoreRefreshedTokensAsync(context, authResult, result.Tokens!);

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
                        timeProvider.GetUtcNow() + _oidcOptions.TokenRefresh.RefreshRetryBackoff;
                    semaphores.RetryNotBefore[sessionKey] = retryAt;
                    logger.LogWarning(
                        "Token refresh deferred: IdP unavailable, session kept, retry after {RetryAt:o} for session={SessionKey}",
                        retryAt,
                        sessionKey
                    );
                    break;

                case TokenRefreshOutcome.Rejected:
                    MarkSessionRejected(context);
                    await SignOutAsync(context);
                    break;
            }
        }
        finally
        {
            semaphore.Release();
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

    private static void MarkSessionRejected(HttpContext context) =>
        context.Items[BffRequestItems.SessionRejected] = true;

    private static Task SignOutAsync(HttpContext context) =>
        context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
