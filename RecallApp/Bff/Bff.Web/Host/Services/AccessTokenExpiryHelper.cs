using System.Globalization;

namespace Host.Services;

/// <summary>
/// Хелпер для расчётов времени истечения токена доступа
/// </summary>
public static class AccessTokenExpiryHelper
{
    /// <summary>Имя сохраняемого токена-метки: момент истечения access-токена (уже за вычетом буфера).</summary>
    public const string TokenName = "expires_at";

    /// <summary>Момент, после которого BFF считает access истёкшим.</summary>
    public static DateTimeOffset Effective(DateTimeOffset actualExpiry, TimeSpan clockSkewBuffer) =>
        actualExpiry - clockSkewBuffer;

    /// <summary>Формат, в котором expires_at пишет OpenIdConnectHandler.SaveTokens: ISO 8601 "o".</summary>
    public static string Format(DateTimeOffset expiresAt) =>
        expiresAt.ToString("o", CultureInfo.InvariantCulture);

    public static bool TryParse(string? raw, out DateTimeOffset expiresAt) =>
        DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out expiresAt);
}
