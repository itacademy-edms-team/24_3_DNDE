using Host.Configuration;
using Microsoft.AspNetCore.Authentication;

namespace Host.Infrastructure;

/// <summary>
/// Политика когерентности cookie BFF с сессией Keycloak (ADR 0027).
/// Срок и персистентность cookie выравниваются на SSO Idle выбранного режима.
/// Режим ("запомнить меня") определяется на входе по длине жизни refresh-токена
/// и хранится маркером в <see cref="AuthenticationProperties"/>, переживая refresh.
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
    /// Ставим оба поля: встроенное скольжение cookie продлевает окно длиной ExpiresUtc - IssuedUtc,
    /// и если сдвинуть только ExpiresUtc, окно расширится.
    /// <para>
    /// Правило для вызывающих: любой ручной SignInAsync на cookie-схеме должен перед этим вызвать
    /// ResetWindow. Если в запросе был SignInAsync, фреймворк отменяет запланированное встроенное
    /// продление (CookieAuthenticationHandler.FinishResponseAsync: «only renew if neither sign in
    /// or sign out was called»), и в cookie уходят те даты, что лежат в переданных properties.
    /// Без ResetWindow запрос не засчитается как активность.
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
