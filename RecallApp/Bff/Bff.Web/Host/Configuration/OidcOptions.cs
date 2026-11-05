using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Distributed;

namespace Host.Configuration;

/// <summary>
/// Настройки OIDC-клиента BFF (Keycloak). Секция "Oidc" в конфигурации.
/// </summary>
public sealed class OidcOptions
{
    public const string SectionName = "Oidc";

    /// <summary>Базовый URL Keycloak с realm (issuer).</summary>
    [Required]
    public string Authority { get; set; } = string.Empty;

    /// <summary>ClientId конфиденциального OIDC-клиента BFF.</summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Секрет OIDC-клиента. Подаётся через env.</summary>
    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Настройки времени жизни access-токена.
    /// </summary>
    public TokenRefreshOptions TokenRefresh { get; set; } = new();

    /// <summary>
    /// Настройки времени жизни SSO сессии в Keycloak
    /// </summary>
    public SsoSessionOptions SsoSession { get; set; } = new();

    /// <summary>
    /// Предел памяти серверного хранилища сессий (байт).
    /// </summary>
    public long SessionStoreSizeLimitBytes { get; set; } = 200L * 1024 * 1024;
}
