using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Host.Configuration;
using Host.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Host.Services;

public interface ITokenRefreshService
{
    Task TryRefreshTokenAsync(HttpContext context);
}

/// <summary>
/// Сервис обновления access-токена.
/// Обновление сериализуется на BFF-сессию.
/// Отказ Keycloak (400/401) делает sign-out серверной сессии;
/// временный сбой (5xx, таймаут, сеть, битый ответ) сессию сохраняет и откладывает повтор на <see cref="TokenLifetimeOptions.RefreshRetryBackoff"/>.
/// </summary>
public sealed class TokenRefreshService(
    IHttpClientFactory httpClientFactory,
    IOptions<OidcOptions> oidcOptions,
    ILogger<TokenRefreshService> logger,
    TokenRefreshSemaphores semaphores
) : ITokenRefreshService
{
    public const string HttpClientName = "keycloak-refresh";

    /// <summary>
    /// Ключ в <see cref="HttpContext.Items"/>, куда кладётся новейший access-токен после обновления в этом же запросе.
    /// GetTokenAsync после SignInAsync отдаёт старый снимок (кэш аутентификации на запрос),
    /// поэтому downstream (будущий token exchange) должен читать свежий токен отсюда.
    /// </summary>
    public const string AccessTokenItemKey = "bff_access_token";

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

        var sub =
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? "unknown";

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

        var expiresAtRaw = authResult.Properties.GetTokenValue("expires_at");
        if (string.IsNullOrEmpty(expiresAtRaw))
        {
            logger.LogWarning(
                "Token refresh skipped: expires_at not found in session {SessionKey} (sub={Sub})",
                sessionKey,
                sub
            );
            return;
        }

        if (
            !DateTimeOffset.TryParse(
                expiresAtRaw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var expiresAt
            )
        )
        {
            logger.LogWarning(
                "Token refresh skipped: failed to parse expires_at='{ExpiresAt}' (sub={Sub})",
                expiresAtRaw,
                sub
            );
            return;
        }

        var refreshThreshold = _oidcOptions.TokenLifetime.RefreshThreshold;
        var timeUntilExpiry = expiresAt - DateTimeOffset.UtcNow;
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
                && DateTimeOffset.UtcNow - lastRefreshed < refreshThreshold
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

            var outcome = await RefreshAndStoreTokensAsync(
                    context,
                    refreshToken,
                    _oidcOptions.TokenLifetime.ClockSkewBuffer
                )
                .ConfigureAwait(false);
            switch (outcome)
            {
                case RefreshOutcome.Success:
                    semaphores.LastRefreshedAt[sessionKey] = DateTimeOffset.UtcNow;
                    semaphores.RetryNotBefore.TryRemove(sessionKey, out _);
                    logger.LogInformation(
                        "Token refresh succeeded for session={SessionKey}",
                        sessionKey
                    );

                    // Ленивый guard: страховка от всплеска логинов между проходами уборщика.
                    if (semaphores.Semaphores.Count > SessionLockSoftCap)
                        semaphores.TrimOldest(LazyTrimBatch);
                    break;

                case RefreshOutcome.Unavailable:
                    // Сессию не трогаем и LastRefreshedAt не ставим: после паузы попытка повторится.
                    var retryAt =
                        DateTimeOffset.UtcNow + _oidcOptions.TokenLifetime.RefreshRetryBackoff;
                    semaphores.RetryNotBefore[sessionKey] = retryAt;
                    logger.LogWarning(
                        "Token refresh deferred: IdP unavailable, session kept, retry after {RetryAt:o} for session={SessionKey}",
                        retryAt,
                        sessionKey
                    );
                    break;

                case RefreshOutcome.Rejected:
                    // Sign-out уже выполнен в RefreshAndStoreTokensAsync.
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
            || DateTimeOffset.UtcNow >= retryNotBefore
        )
            return false;

        logger.LogDebug(
            "Token refresh skipped (retry backoff until {RetryNotBefore:o}): session={SessionKey}",
            retryNotBefore,
            sessionKey
        );
        return true;
    }

    private async Task<RefreshOutcome> RefreshAndStoreTokensAsync(
        HttpContext context,
        string refreshToken,
        TimeSpan clockSkewBuffer
    )
    {
        var (outcome, tokenResponse) = await RequestNewTokensAsync(refreshToken)
            .ConfigureAwait(false);
        switch (outcome)
        {
            case RefreshOutcome.Rejected:
                await SignOutAsync(context).ConfigureAwait(false);
                return outcome;
            case RefreshOutcome.Unavailable:
                return outcome;
        }

        await StoreNewTokensAsync(
                context,
                tokenResponse!,
                refreshToken,
                clockSkewBuffer,
                _oidcOptions.SsoSession
            )
            .ConfigureAwait(false);
        return RefreshOutcome.Success;
    }

    /// <summary>
    /// Запрос к token-endpoint. При <see cref="RefreshOutcome.Success"/> документ ответа не null
    /// и содержит access_token и expires_in; владение документом переходит вызывающему.
    /// </summary>
    private async Task<(RefreshOutcome Outcome, JsonDocument? Tokens)> RequestNewTokensAsync(
        string refreshToken
    )
    {
        var tokenEndpoint = $"{_oidcOptions.Authority}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _oidcOptions.ClientId,
            ["client_secret"] = _oidcOptions.ClientSecret,
            ["refresh_token"] = refreshToken,
        };

        var http = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var response = await http.PostAsync(
                    tokenEndpoint,
                    new FormUrlEncodedContent(data)
                )
                .ConfigureAwait(false);

            // 400 (invalid_grant: refresh отозван или истёк) и 401 (неверный клиент или секрет)
            // это окончательный отказ: повтор того же запроса не поможет.
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                logger.LogWarning(
                    "Token refresh rejected by IdP with status {Status}",
                    response.StatusCode
                );
                return (RefreshOutcome.Rejected, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Token refresh failed with transient status {Status}",
                    response.StatusCode
                );
                return (RefreshOutcome.Unavailable, null);
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var document = JsonDocument.Parse(json);
            if (!HasRequiredTokenFields(document.RootElement))
            {
                document.Dispose();
                logger.LogWarning(
                    "Token refresh response lacks access_token or expires_in"
                );
                return (RefreshOutcome.Unavailable, null);
            }

            return (RefreshOutcome.Success, document);
        }
        catch (Exception ex)
            when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Сеть, таймаут HttpClient (TaskCanceledException) или битый JSON.
            logger.LogWarning(ex, "Token refresh request failed");
            return (RefreshOutcome.Unavailable, null);
        }
    }

    private static bool HasRequiredTokenFields(JsonElement root) =>
        root.ValueKind == JsonValueKind.Object
        && root.TryGetProperty("access_token", out var accessToken)
        && accessToken.ValueKind == JsonValueKind.String
        && root.TryGetProperty("expires_in", out var expiresIn)
        && expiresIn.ValueKind == JsonValueKind.Number
        && expiresIn.TryGetInt32(out _);

    private static async Task StoreNewTokensAsync(
        HttpContext context,
        JsonDocument tokenResponse,
        string fallbackRefreshToken,
        TimeSpan clockSkewBuffer,
        SsoSessionOptions sso
    )
    {
        using (tokenResponse)
        {
            var root = tokenResponse.RootElement;

            var authResult = await context
                .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .ConfigureAwait(false);
            if (!authResult.Succeeded || authResult.Principal is null)
                return;

            var newAccessToken = root.GetProperty("access_token").GetString();
            var newRefreshToken = root.TryGetProperty("refresh_token", out var rProp)
                ? rProp.GetString()
                : fallbackRefreshToken;
            var newIdToken = root.TryGetProperty("id_token", out var idProp)
                ? idProp.GetString()
                : await context.GetTokenAsync("id_token").ConfigureAwait(false);

            var expiresInSec = root.GetProperty("expires_in").GetInt32();
            var newExpiresAt = DateTimeOffset
                .UtcNow.Add(TimeSpan.FromSeconds(expiresInSec) - clockSkewBuffer)
                .ToString("o", CultureInfo.InvariantCulture);

            authResult.Properties!.StoreTokens(
                [
                    new() { Name = "access_token", Value = newAccessToken! },
                    new() { Name = "refresh_token", Value = newRefreshToken! },
                    new() { Name = "id_token", Value = newIdToken! },
                    new() { Name = "expires_at", Value = newExpiresAt },
                ]
            );

            // Когерентность cookie (ADR 0027).
            // SignInAsync ниже отменяет встроенное скольжение cookie в этом запросе (FinishResponseAsync не продлевает, если был SignIn),
            // и в cookie уйдут даты из authResult.Properties.
            // Поэтому окно начинаем заново сами:
            // IssuedUtc = now, ExpiresUtc = now + SSO Idle режима (режим по маркеру bff_remember_me).
            SessionCookieLifetime.ResetWindow(authResult.Properties!, sso, DateTimeOffset.UtcNow);

            await context
                .SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    authResult.Properties
                )
                .ConfigureAwait(false);

            // Мостик актуальности в пределах этого запроса: GetTokenAsync ниже по конвейеру вернул бы старый снимок из-за кэша аутентификации,
            // поэтому новейший access кладём в Items для downstream-потребителей.
            context.Items[AccessTokenItemKey] = newAccessToken;
        }
    }

    private static Task SignOutAsync(HttpContext context) =>
        context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    /// <summary>Исход попытки refresh.</summary>
    private enum RefreshOutcome
    {
        /// <summary>Токены получены и сохранены.</summary>
        Success,

        /// <summary>Keycloak отказал окончательно (400/401): сессия завершается.</summary>
        Rejected,

        /// <summary>Временный сбой (5xx, таймаут, сеть, битый ответ): сессия сохраняется, повтор после паузы.</summary>
        Unavailable,
    }
}
