using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FinanceTrack.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace FinanceTrack.Gateway.Services;

public interface ITokenRefreshService
{
    Task TryRefreshTokenAsync(HttpContext context);
}

public sealed class TokenRefreshService(
    IHttpClientFactory httpClientFactory,
    IOptions<OidcOptions> oidcOptions,
    ILogger<TokenRefreshService> logger,
    TokenRefreshSemaphores semaphores
) : ITokenRefreshService
{
    public const string HttpClientName = "keycloak-refresh";
    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    public async Task TryRefreshTokenAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        var sub =
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context
                .User.FindFirst("sub")
                ?.Value;
        if (string.IsNullOrEmpty(sub))
        {
            logger.LogWarning(
                "Token refresh skipped: no subject claim found. Claims present: {Claims}",
                string.Join(", ", context.User.Claims.Select(c => c.Type))
            );
            return;
        }

        var expiresAtRaw = await context.GetTokenAsync("expires_at");
        if (string.IsNullOrEmpty(expiresAtRaw))
        {
            logger.LogWarning(
                "Token refresh skipped: expires_at not found in cookie for sub={Sub}",
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
                "Token refresh skipped: failed to parse expires_at='{ExpiresAt}' for sub={Sub}",
                expiresAtRaw,
                sub
            );
            return;
        }

        var refreshThreshold = _oidcOptions.TokenLifetime.RefreshThreshold;
        var timeUntilExpiry = expiresAt - DateTimeOffset.UtcNow;
        logger.LogDebug(
            "Token refresh check: expires_at={ExpiresAt}, timeUntilExpiry={TimeUntilExpiry:g}, threshold={Threshold:g}, sub={Sub}",
            expiresAt,
            timeUntilExpiry,
            refreshThreshold,
            sub
        );

        if (timeUntilExpiry > refreshThreshold)
            return;

        var lockKey = $"token_refresh:{sub}";
        var sem = semaphores.Semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (
                semaphores.LastRefreshedAt.TryGetValue(sub, out var lastRefreshed)
                && DateTimeOffset.UtcNow - lastRefreshed < refreshThreshold
            )
            {
                logger.LogDebug(
                    "Token refresh skipped (double-check): lastRefreshed={LastRefreshed}, sub={Sub}",
                    lastRefreshed,
                    sub
                );
                return;
            }

            logger.LogInformation(
                "Performing token refresh for sub={Sub}, tokenExpiry={ExpiresAt}",
                sub,
                expiresAt
            );

            var refreshToken = await context.GetTokenAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("Token refresh failed: refresh_token missing for sub={Sub}", sub);
                await SignOutAsync(context);
                return;
            }

            var succeeded = await RefreshAndStoreTokensAsync(
                context,
                refreshToken,
                _oidcOptions.TokenLifetime.ClockSkewBuffer
            );
            if (succeeded)
            {
                semaphores.LastRefreshedAt[sub] = DateTimeOffset.UtcNow;
                logger.LogInformation("Token refresh succeeded for sub={Sub}", sub);
            }
        }
        finally
        {
            sem.Release();
        }
    }

    private async Task<bool> RefreshAndStoreTokensAsync(
        HttpContext context,
        string refreshToken,
        TimeSpan clockSkewBuffer
    )
    {
        var tokenResponse = await RequestNewTokensAsync(refreshToken);
        if (tokenResponse == null)
        {
            await SignOutAsync(context);
            return false;
        }

        await StoreNewTokensAsync(context, tokenResponse, refreshToken, clockSkewBuffer);
        return true;
    }

    private async Task<JsonDocument?> RequestNewTokensAsync(string refreshToken)
    {
        var options = oidcOptions.Value;
        var tokenEndpoint = $"{options.Bff.Authority}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = options.Bff.ClientId,
            ["client_secret"] = options.Bff.ClientSecret,
            ["refresh_token"] = refreshToken,
        };

        var http = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            var response = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(data));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Token refresh failed with status {Status}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token refresh request failed");
            return null;
        }
    }

    private static async Task StoreNewTokensAsync(
        HttpContext context,
        JsonDocument tokenResponse,
        string fallbackRefreshToken,
        TimeSpan clockSkewBuffer
    )
    {
        using (tokenResponse)
        {
            var root = tokenResponse.RootElement;

            var authResult = await context.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            if (!authResult.Succeeded || authResult.Principal is null)
                return;

            var newAccessToken = root.GetProperty("access_token").GetString();
            var newRefreshToken = root.TryGetProperty("refresh_token", out var rProp)
                ? rProp.GetString()
                : fallbackRefreshToken;
            var newIdToken = root.TryGetProperty("id_token", out var idProp)
                ? idProp.GetString()
                : await context.GetTokenAsync("id_token");

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

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authResult.Principal,
                authResult.Properties
            );

            context.Items["bff_access_token"] = newAccessToken;
        }
    }

    private static Task SignOutAsync(HttpContext context) =>
        context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
