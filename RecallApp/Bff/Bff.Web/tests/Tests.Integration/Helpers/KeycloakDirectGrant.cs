using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Tests.Integration.Helpers;

/// <summary>Ответ token-endpoint Keycloak.</summary>
public sealed record KeycloakTokens(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);

/// <summary>
/// Прямые вызовы token-endpoint тестового realm: вход по паролю (Direct Access Grants)
/// и refresh в обход BFF. Для засевания сессии и подготовки состояний Keycloak.
/// </summary>
public static class KeycloakDirectGrant
{
    private static readonly HttpClient Http = new();

    /// <summary>Вход testuser через Direct Access Grants: новая сессия Keycloak и её токены.</summary>
    public static Task<KeycloakTokens> LoginAsync(KeycloakTestSettings settings) =>
        RequestAsync(
            settings,
            new()
            {
                ["grant_type"] = "password",
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret,
                ["username"] = settings.Username,
                ["password"] = settings.Password,
                ["scope"] = "openid",
            }
        );

    /// <summary>Refresh в обход BFF: использует и тем самым отзывает переданный refresh-токен.</summary>
    public static Task<KeycloakTokens> RefreshAsync(KeycloakTestSettings settings, string refreshToken) =>
        RequestAsync(
            settings,
            new()
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret,
                ["refresh_token"] = refreshToken,
            }
        );

    private static async Task<KeycloakTokens> RequestAsync(
        KeycloakTestSettings settings,
        Dictionary<string, string> form
    )
    {
        using var response = await Http.PostAsync(settings.TokenEndpoint, new FormUrlEncodedContent(form));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Keycloak token endpoint returned {(int)response.StatusCode}: {error}"
            );
        }

        return (await response.Content.ReadFromJsonAsync<KeycloakTokens>())!;
    }
}
