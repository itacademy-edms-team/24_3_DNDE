using Host.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Host.Controllers;

/// <summary>
/// Контроллер аутентификации и состояния серверной сессии BFF:
/// инициирует вход/выход редиректами и отвечает SPA, жива ли сессия.
/// </summary>
[ApiController]
public class AuthController(IOptions<SpaOptions> spa) : ControllerBase
{
    private readonly SpaOptions _spa = spa.Value;

    /// <summary>
    /// Комбинирует SPA-адрес и <paramref name="returnUrl"/>.
    /// Предусмотрена проверка адреса на нелокальность, если адрес полный, то fallback на "/".
    /// </summary>
    /// <param name="returnUrl">Путь, полученный при переходе из SPA.</param>
    /// <returns>Полный адрес SPA и добавленный локальный путь.</returns>
    private string ToSpaUrl(string? returnUrl)
    {
        var path = returnUrl is not null && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

        // BaseUrl без хвостового слэша (валидатор), path начинается со слэша (IsLocalUrl).
        return $"{_spa.BaseUrl}{path}";
    }

    /// <summary>При попадании на корень выполняем редирект на SPA.</summary>
    [HttpGet("/")]
    public IActionResult Root() => Redirect(ToSpaUrl("/"));

    /// <summary>
    /// Инициирует вход через Keycloak (redirect на хостовую страницу).
    /// При уже открытой сессии повторный вход не начинается: сразу redirect на returnUrl.
    /// </summary>
    /// <param name="returnUrl">Локальный путь возврата после входа. Внешние URL отклоняются.</param>
    [HttpGet("/login")]
    public IActionResult Login([FromQuery] string? returnUrl = "/")
    {
        var target = ToSpaUrl(returnUrl);
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(target);

        return Challenge(
            new AuthenticationProperties { RedirectUri = target },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Выход из аккаунта: сброс серверной сессии (cookie) и OIDC-сессии Keycloak.
    /// Только POST: GET-навигация верхнего уровня проходит SameSite=Lax,
    /// и сторонний сайт разлогинил бы пользователя картинкой-триггером, например.
    /// Без живой сессии делаем только локальный redirect, в Keycloak не ходим.
    /// </summary>
    [HttpPost("/logout")]
    public IActionResult Logout()
    {
        var target = ToSpaUrl("/");
        if (User.Identity?.IsAuthenticated != true)
            return Redirect(target);

        return SignOut(
            new AuthenticationProperties { RedirectUri = target },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Проба живости серверной сессии для SPA: жива ли cookie, и кто в ней.
    /// В монолит не ходит: недоступность App не должна выглядеть как разлогин.
    /// Роли и sub здесь не отдаются - это диагностика, см. /debug/session.
    /// </summary>
    [HttpGet("/session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // Состояние сессии не кэшируется: bfcache/back-навигация иначе отдавали бы устаревшее состояние.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Session()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new { isAuthenticated = false });

        // Внутренние идентификаторы (sid, session_state и пр.) наружу не отдаём.
        return Ok(
            new
            {
                isAuthenticated = true,
                name = User.Identity.Name,
                email = User.FindFirst("email")?.Value,
            }
        );
    }
}
