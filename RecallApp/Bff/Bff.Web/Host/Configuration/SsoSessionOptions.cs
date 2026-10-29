namespace Host.Configuration;

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
    /// Потолок времени жизни сессии, которое она может прожить в простое.
    /// Сбрасывается каждый раз, когда выполняется token-refresh.
    /// Применяется, если во время входа отметить флаг "Remember Me".
    /// </summary>
    public TimeSpan SessionIdleRememberMe { get; set; } = TimeSpan.FromDays(30);
}
