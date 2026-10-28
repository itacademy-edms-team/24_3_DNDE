using System.Net;
using System.Text.Json;
using Host.Configuration;
using Microsoft.Extensions.Options;

namespace Host.Services;

/// <summary>Исход попытки refresh.</summary>
public enum TokenRefreshOutcome
{
    /// <summary>Токены получены.</summary>
    Success,

    /// <summary>Keycloak отказал окончательно (400/401): сессия завершается.</summary>
    Rejected,

    /// <summary>Временный сбой (5xx, таймаут, сеть, битый ответ): сессия сохраняется, повтор после паузы.</summary>
    Unavailable,
}

/// <summary>
/// Токены из ответа token-endpoint. Если в ответе refresh_token, id_token или token_type равны null, тогда остаются прежние значения сессии.
/// </summary>
public sealed record RefreshedTokens(
    string AccessToken,
    string? RefreshToken,
    string? IdToken,
    string? TokenType,
    TimeSpan ExpiresIn
);

/// <summary>Результат обращения к token-endpoint. <see cref="Tokens"/> не null только при Success.</summary>
public sealed record TokenRefreshResult(TokenRefreshOutcome Outcome, RefreshedTokens? Tokens = null);

/// <summary>
/// HTTP-клиент token-endpoint Keycloak для grant_type=refresh_token.
/// Знает только протокол: отправляет запрос и классифицирует ответ в <see cref="TokenRefreshOutcome"/>.
/// Сессию BFF не трогает.
/// </summary>
public sealed class KeycloakTokenClient(
    HttpClient http,
    IOptions<OidcOptions> oidcOptions,
    ILogger<KeycloakTokenClient> logger
)
{
    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    public async Task<TokenRefreshResult> RefreshAsync(string refreshToken)
    {
        var tokenEndpoint = $"{_oidcOptions.Authority}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _oidcOptions.ClientId,
            ["client_secret"] = _oidcOptions.ClientSecret,
            ["refresh_token"] = refreshToken,
        };

        try
        {
            using var response = await http.PostAsync(
                    tokenEndpoint,
                    new FormUrlEncodedContent(data)
                );

            // 400 (invalid_grant: refresh отозван или истёк) и 401 (неверный клиент или секрет)
            // это окончательный отказ: повтор того же запроса не поможет.
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                logger.LogWarning(
                    "Token refresh rejected by IdP with status {Status}",
                    response.StatusCode
                );
                return new TokenRefreshResult(TokenRefreshOutcome.Rejected);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Token refresh failed with transient status {Status}",
                    response.StatusCode
                );
                return new TokenRefreshResult(TokenRefreshOutcome.Unavailable);
            }

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var tokens = ReadTokens(document.RootElement);
            if (tokens is null)
            {
                logger.LogWarning("Token refresh response lacks access_token or expires_in");
                return new TokenRefreshResult(TokenRefreshOutcome.Unavailable);
            }

            return new TokenRefreshResult(TokenRefreshOutcome.Success, tokens);
        }
        catch (Exception ex)
            when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Сеть, таймаут HttpClient (TaskCanceledException) или битый JSON.
            logger.LogWarning(ex, "Token refresh request failed");
            return new TokenRefreshResult(TokenRefreshOutcome.Unavailable);
        }
    }

    /// <summary>Достаёт токены из ответа; null, если нет обязательных access_token и expires_in.</summary>
    private static RefreshedTokens? ReadTokens(JsonElement root)
    {
        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("access_token", out var accessToken)
            || accessToken.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("expires_in", out var expiresIn)
            || expiresIn.ValueKind != JsonValueKind.Number
            || !expiresIn.TryGetInt32(out var expiresInSec)
        )
            return null;

        return new RefreshedTokens(
            accessToken.GetString()!,
            ReadOptionalString(root, "refresh_token"),
            ReadOptionalString(root, "id_token"),
            ReadOptionalString(root, "token_type"),
            TimeSpan.FromSeconds(expiresInSec)
        );
    }

    /// <summary>
    /// Необязательное строковое поле. Значение другого типа считаем отсутствующим:
    /// GetString() на нём бросил бы InvalidOperationException мимо классификации исходов.
    /// </summary>
    private static string? ReadOptionalString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
