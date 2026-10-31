using Host.Configuration;
using Microsoft.AspNetCore.Authentication;

namespace Host.Infrastructure;

/// <summary>
/// Вычисляет срок жизни и персистентность cookie BFF-сессии по режиму сессии.
/// Срок выравнивается на SSO Idle выбранного режима (обычный или «запомнить меня»).
/// Режим определяется на входе по длине жизни refresh-токена и хранится маркером
/// в <see cref="AuthenticationProperties"/>, переживая refresh.
/// </summary>
public static class SessionCookieLifetime
{
    /// <summary>Ключ маркера режима «запомнить меня» в properties.</summary>
    public const string RememberMeKey = "bff_remember_me";

    /// <summary>Resolve SSO Idle соответствующего режима.</summary>
    public static TimeSpan ResolveIdle(bool rememberMe, SsoSessionOptions sso) =>
        rememberMe ? sso.SessionIdleRememberMe : sso.SessionIdle;

    /// <summary>
    /// Применить режим к cookie на входе: персистентность, маркер и срок по SSO Idle режима.
    /// </summary>
    public static void Apply(
        AuthenticationProperties properties,
        bool rememberMe,
        SsoSessionOptions sso,
        DateTimeOffset now
    )
    {
        properties.IsPersistent = rememberMe;
        properties.SetString(RememberMeKey, rememberMe ? "1" : "0");
        ResetWindow(properties, sso, now);
    }

    /// <summary>
    /// Начать окно сессии заново: IssuedUtc = now, ExpiresUtc = now + SSO Idle режима.
    /// <para>
    /// Встроенное скольжение cookie выключено, поэтому после входа окно сдвигается только здесь.
    /// Ручной SignInAsync на cookie-схеме делается только через <see cref="Host.Services.SessionTokenWriter"/>,
    /// который вызывает ResetWindow перед SignIn: иначе в cookie и стор уйдут
    /// старые даты из переданных properties и запрос не засчитается как активность.
    /// </para>
    /// </summary>
    public static void ResetWindow(
        AuthenticationProperties properties,
        SsoSessionOptions sso,
        DateTimeOffset now
    )
    {
        properties.IssuedUtc = now;
        properties.ExpiresUtc = now + ResolveIdle(ReadRememberMe(properties), sso);
    }

    /// <summary>Прочитать режим из properties (по умолчанию false).</summary>
    public static bool ReadRememberMe(AuthenticationProperties properties) =>
        properties.GetString(RememberMeKey) == "1";
}
