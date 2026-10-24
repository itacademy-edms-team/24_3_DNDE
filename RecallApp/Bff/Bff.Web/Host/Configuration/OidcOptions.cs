using System.ComponentModel.DataAnnotations;

namespace Host.Configuration;

/// <summary>
/// Настройки OIDC-клиента BFF (Keycloak). Секция "Oidc" в конфигурации.
/// ClientSecret приходит из окружения (.env), не хранится в репозитории.
/// </summary>
public sealed class OidcOptions
{
    public const string SectionName = "Oidc";

    [Required]
    public string Authority { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Настройки времени жизни access-токена.
    /// </summary>
    public TokenLifetimeOptions TokenLifetime { get; set; } = new();
}

/// <summary>
/// Параметры упреждающего обновления токена в BFF.
/// </summary>
public sealed class TokenLifetimeOptions
{
    /// <summary>
    /// За сколько до истечения access-токена его обновлять.
    /// Обновление запускается, когда времени до истечения меньше этого порога.
    /// </summary>
    public TimeSpan RefreshThreshold { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Буфер на рассинхрон часов.
    /// Вычитается из времени жизни токена при расчёте expires_at, сохраняемого в cookie.
    /// </summary>
    public TimeSpan ClockSkewBuffer { get; set; } = TimeSpan.FromSeconds(30);
}
