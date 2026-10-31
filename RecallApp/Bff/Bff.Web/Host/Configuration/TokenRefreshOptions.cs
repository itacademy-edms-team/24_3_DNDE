namespace Host.Configuration;

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
    /// Таймаут обращения к Keycloak. Обязан быть меньше <see cref="RefreshThreshold"/>:
    /// иначе ожидание под замком сессии может пережить окно refresh, и баг сценария 9 вернётся.
    /// Проверяется на старте (<see cref="OidcOptionsValidator"/>).
    /// </summary>
    public TimeSpan KeycloakTimeout { get; set; } = TimeSpan.FromSeconds(5);

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
