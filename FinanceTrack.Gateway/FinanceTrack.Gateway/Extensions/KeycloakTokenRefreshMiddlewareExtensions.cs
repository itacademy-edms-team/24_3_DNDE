using System.Globalization;
using System.Text.Json;
using FinanceTrack.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace FinanceTrack.Gateway.Extensions;

/// <summary>
/// Middleware that automatically refreshes Keycloak tokens before they expire
/// </summary>
public static class KeycloakTokenRefreshMiddlewareExtensions
{
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(1);

    public static IApplicationBuilder UseKeycloakTokenRefresh(this IApplicationBuilder app)
    {
        return app.Use(
            async (context, next) =>
            {
                if (await TryRefreshTokenAsync(context))
                {
                    // Token was refreshed, continue with new token
                }

                await next();
            }
        );
    }

    private static async Task<bool> TryRefreshTokenAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (!await ShouldRefreshTokenAsync(context))
        {
            return false;
        }

        var refreshToken = await context.GetTokenAsync("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
        {
            await SignOutAsync(context);
            return false;
        }

        return await RefreshAndStoreTokensAsync(context, refreshToken);
    }

    private static async Task<bool> ShouldRefreshTokenAsync(HttpContext context)
    {
        var expiresAtRaw = await context.GetTokenAsync("expires_at");
        if (string.IsNullOrEmpty(expiresAtRaw))
        {
            return false;
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
            return false;
        }

        var timeLeft = expiresAt - DateTimeOffset.UtcNow;
        return timeLeft <= RefreshThreshold;
    }

    private static async Task<bool> RefreshAndStoreTokensAsync(
        HttpContext context,
        string refreshToken
    )
    {
        var oidcOptions = context.RequestServices.GetRequiredService<IOptions<OidcOptions>>().Value;

        var tokenResponse = await RequestNewTokensAsync(context, oidcOptions, refreshToken);
        if (tokenResponse == null)
        {
            await SignOutAsync(context);
            return false;
        }

        return await StoreNewTokensAsync(context, tokenResponse, refreshToken);
    }

    private static async Task<JsonDocument?> RequestNewTokensAsync(
        HttpContext context,
        OidcOptions oidcOptions,
        string refreshToken
    )
    {
        var tokenEndpoint = $"{oidcOptions.Bff.Authority}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = oidcOptions.Bff.ClientId,
            ["client_secret"] = oidcOptions.Bff.ClientSecret,
            ["refresh_token"] = refreshToken,
        };

        // Use IHttpClientFactory if available, otherwise create client
        var httpClientFactory = context.RequestServices.GetService<IHttpClientFactory>();
        using var http = httpClientFactory?.CreateClient() ?? new HttpClient();

        try
        {
            var response = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(data));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
        catch
        {
            // Keycloak unreachable - continue with existing token
            return null;
        }
    }

    private static async Task<bool> StoreNewTokensAsync(
        HttpContext context,
        JsonDocument tokenResponse,
        string fallbackRefreshToken
    )
    {
        using (tokenResponse)
        {
            var root = tokenResponse.RootElement;

            var authResult = await context.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            if (!authResult.Succeeded || authResult.Principal is null)
            {
                return false;
            }

            var newAccessToken = root.GetProperty("access_token").GetString();
            var newRefreshToken = root.TryGetProperty("refresh_token", out var rProp)
                ? rProp.GetString()
                : fallbackRefreshToken;
            var newIdToken = root.TryGetProperty("id_token", out var idProp)
                ? idProp.GetString()
                : await context.GetTokenAsync("id_token");

            var expiresInSec = root.GetProperty("expires_in").GetInt32();
            var newExpiresAt = DateTimeOffset
                .UtcNow.AddSeconds(expiresInSec - 30)
                .ToString("o", CultureInfo.InvariantCulture);

            var tokens = new List<AuthenticationToken>
            {
                new() { Name = "access_token", Value = newAccessToken! },
                new() { Name = "refresh_token", Value = newRefreshToken! },
                new() { Name = "id_token", Value = newIdToken! },
                new() { Name = "expires_at", Value = newExpiresAt },
            };

            authResult.Properties!.StoreTokens(tokens);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authResult.Principal,
                authResult.Properties
            );

            // Store for use in token exchange transform
            context.Items["bff_access_token"] = newAccessToken;

            return true;
        }
    }

    private static Task SignOutAsync(HttpContext context)
    {
        return context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
