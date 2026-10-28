using System.ComponentModel.DataAnnotations;

namespace Host.Configuration;

/// <summary>
/// Настройки OIDC-клиента BFF (Keycloak). Секция "Oidc" в конфигурации.
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
    public TokenRefreshOptions TokenRefresh { get; set; } = new();

    /// <summary>
    /// Настройки времени жизни SSO сессии в Keycloak
    /// </summary>
    public SsoSessionOptions SsoSession { get; set; } = new();
}

/// <summary>
/// Параметры упреждающего обновления токена в BFF.
/// </summary>
public sealed class TokenRefreshOptions
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

    /// <summary>
    /// Через сколько времени простоя запись замка сессии считается мёртвой и удаляется уборщиком.
    /// Должна быть заметно больше частоты обновлений токена, чтобы активную сессию не удалило между обновлениями.
    /// </summary>
    public TimeSpan SessionLockIdle { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Пауза перед повторной попыткой refresh после временного сбоя Keycloak (5xx, таймаут, сеть).
    /// </summary>
    public TimeSpan RefreshRetryBackoff { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Граница времени жизни refresh-токена, после которого он считается принадлежащим к "Remember Me" сессии.
    /// Смотрим оставшееся время жизни refresh-токена и округляем вверх до целого дня.
    /// Оставшееся время жизни refresh-токена считается, как округлённый вверх минимум из следующих опций:
    /// <list type="bullet">
    /// <item>
    /// <description>SSO Session Idle</description>
    /// </item>
    /// <item>
    /// <description>SSO Session Idle Max - SSO Session Idle</description>
    /// </item>
    /// <item>
    /// <description>SSO Session Idle Remember Me</description>
    /// </item>
    /// <item>
    /// <description>SSO Session Idle Max Remember Me - SSO Session Idle Remember Me</description>
    /// </item>
    /// </list>
    /// </summary>
    public TimeSpan RememberMeThreshold { get; set; } = TimeSpan.FromDays(1);
}

/// <summary>
/// Настройки SSO сессии в Keycloak
/// </summary>
public sealed class SsoSessionOptions
{
    /// <summary>
    /// Потолок времени жизни сессии, которое она может прожить в простое.
    /// Сбрасывается каждый раз, когда выполняется token-refresh.
    /// </summary>
    public TimeSpan SessionIdle { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Максимальное время жизни сессии, которое она может прожить от входа в аккаунт и создания этой сессии.
    /// Не сбрасывается, как <see cref="SessionIdle"/>.
    /// Показывает, сколько максимально сможет прожить сессия с учётом всех token-refresh.
    /// </summary>
    public TimeSpan SessionIdleMax { get; set; } = TimeSpan.FromHours(10);

    /// <summary>
    /// Потолок времени жизни сессии, которое она может прожить в простое.
    /// Сбрасывается каждый раз, когда выполняется token-refresh.
    /// Применяется, если во время входа отметить флаг "Remember Me".
    /// </summary>
    public TimeSpan SessionIdleRememberMe { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Максимальное время жизни сессии, которое она может прожить от входа в аккаунт и создания этой сессии.
    /// Не сбрасывается, как <see cref="SessionIdle"/>.
    /// Показывает, сколько максимально сможет прожить сессия с учётом всех token-refresh.
    /// Применяется, если во время входа отметить флаг "Remember Me".
    /// </summary>
    public TimeSpan SessionIdleRememberMeMax { get; set; } = TimeSpan.FromDays(90);
}
