using System.Net;
using System.Text.Json;
using Host.Configuration;
using Microsoft.Extensions.Options;

namespace Host.Services;

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

    /// <summary>
    /// Обменять refresh-токен на новую пару токенов.
    /// Не бросает исключений по сетевым/протокольным ошибкам: они - в исходе результата.
    /// Отмена через <paramref name="cancellationToken"/> пробрасывается как OperationCanceledException.
    /// </summary>
    public async Task<TokenRefreshResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
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

        try
        {
            using var response = await http.PostAsync(
                    tokenEndpoint,
                    new FormUrlEncodedContent(data),
                    cancellationToken
                );

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                var error = await TryReadErrorAsync(response, cancellationToken);

                // Окончательный отказ только для invalid_grant: refresh отозван или истёк.
                if (response.StatusCode == HttpStatusCode.BadRequest && error == "invalid_grant")
                {
                    logger.LogWarning(
                        "Token refresh rejected by IdP with status {Status}",
                        response.StatusCode
                    );
                    return new TokenRefreshResult(TokenRefreshOutcome.Rejected);
                }

                // 401 и 400 с иной ошибкой (invalid_client, unauthorized_client, ...): сбой конфигурации или клиента, а не сессии.
                // Разлогинивать всех не нужно: ведём себя как при недоступном Keycloak - сессия жива, ретрай после паузы.
                logger.LogError(
                    "Token refresh failed with status {Status} and error {Error}: treating as IdP unavailability, session kept",
                    response.StatusCode,
                    error
                );
                return new TokenRefreshResult(TokenRefreshOutcome.Unavailable);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Token refresh failed with transient status {Status}",
                    response.StatusCode
                );
                return new TokenRefreshResult(TokenRefreshOutcome.Unavailable);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
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
            when (
                ex is HttpRequestException or JsonException
                || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            )
        {
            // Сеть, таймаут HttpClient (TaskCanceledException) или битый JSON.
            // Отмена самого запроса пробрасывается выше.
            logger.LogWarning(ex, "Token refresh request failed");
            return new TokenRefreshResult(TokenRefreshOutcome.Unavailable);
        }
    }

    /// <summary>Поле error из JSON-тела ответа; null, если тело не JSON или поля нет.</summary>
    private static async Task<string?> TryReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.String
                ? error.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
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
