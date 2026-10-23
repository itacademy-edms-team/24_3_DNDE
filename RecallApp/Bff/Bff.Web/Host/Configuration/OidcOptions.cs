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
}
