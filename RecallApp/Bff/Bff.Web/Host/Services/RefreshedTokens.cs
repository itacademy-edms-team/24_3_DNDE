namespace Host.Services;

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
