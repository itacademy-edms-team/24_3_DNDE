using System.Text.Json;
using FinanceTrack.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace FinanceTrack.Gateway.Extensions
{
    public static class KeycloakTokenRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseKeycloakTokenRefresh(this IApplicationBuilder app)
        {
            return app.Use(
                async (context, next) =>
                {
                    // No cookie auth — no refresh
                    if (context.User?.Identity?.IsAuthenticated != true)
                    {
                        await next();
                        return;
                    }

                    // Check expires_at
                    var expiresAtRaw = await context.GetTokenAsync("expires_at");
                    if (string.IsNullOrEmpty(expiresAtRaw))
                    {
                        await next();
                        return;
                    }

                    if (!DateTimeOffset.TryParse(expiresAtRaw, out var expiresAt))
                    {
                        await next();
                        return;
                    }

                    var now = DateTimeOffset.UtcNow;
                    var timeLeft = expiresAt - now;

                    if (timeLeft > TimeSpan.FromMinutes(1))
                    {
                        await next();
                        return;
                    }

                    // Check refresh_token
                    var refreshToken = await context.GetTokenAsync("refresh_token");
                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        // no refresh token — sign out
                        await context.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme
                        );
                        await next();
                        return;
                    }

                    var oidcOptions = context
                        .RequestServices.GetRequiredService<IOptions<OidcOptions>>()
                        .Value;

                    var tokenEndpoint =
                        $"{oidcOptions.Bff.Authority}/protocol/openid-connect/token";

                    var data = new Dictionary<string, string>
                    {
                        ["grant_type"] = "refresh_token",
                        ["client_id"] = oidcOptions.Bff.ClientId,
                        ["client_secret"] = oidcOptions.Bff.ClientSecret,
                        ["refresh_token"] = refreshToken,
                    };

                    using var http = new HttpClient();
                    HttpResponseMessage resp;

                    try
                    {
                        resp = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(data));
                    }
                    catch
                    {
                        // If keycloak is unreachable, just continue with the old token
                        await next();
                        return;
                    }

                    if (!resp.IsSuccessStatusCode)
                    {
                        // refresh failed with status code - sign out
                        await context.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme
                        );
                        await next();
                        return;
                    }

                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var newAccessToken = root.GetProperty("access_token").GetString();
                    var newRefreshToken = root.TryGetProperty("refresh_token", out var rProp)
                        ? rProp.GetString()
                        : refreshToken;
                    var newIdToken = root.TryGetProperty("id_token", out var idProp)
                        ? idProp.GetString()
                        : await context.GetTokenAsync("id_token");

                    var expiresInSec = root.GetProperty("expires_in").GetInt32();
                    var newExpiresAt = DateTimeOffset
                        .UtcNow.AddSeconds(expiresInSec - 30)
                        .ToString("o");

                    // get current auth ticket
                    var authResult = await context.AuthenticateAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );
                    if (!authResult.Succeeded || authResult.Principal is null)
                    {
                        await next();
                        return;
                    }

                    var tokens = new List<AuthenticationToken>
                    {
                        new AuthenticationToken { Name = "access_token", Value = newAccessToken! },
                        new AuthenticationToken
                        {
                            Name = "refresh_token",
                            Value = newRefreshToken!,
                        },
                        new AuthenticationToken { Name = "id_token", Value = newIdToken! },
                        new AuthenticationToken { Name = "expires_at", Value = newExpiresAt },
                    };

                    authResult.Properties!.StoreTokens(tokens);

                    // re-sign-in with updated tokens
                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        authResult.Principal,
                        authResult.Properties
                    );

                    await next();
                }
            );
        }
    }
}
