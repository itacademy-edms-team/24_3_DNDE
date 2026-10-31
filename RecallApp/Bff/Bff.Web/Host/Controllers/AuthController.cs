using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Host.Controllers;

/// <summary>
/// Контроллер аутентификации и состояния серверной сессии BFF:
/// инициирует вход/выход редиректами и отдаёт клиенту состояние сессии и client-роли RecallApp.
/// </summary>
[ApiController]
public class AuthController : ControllerBase
{
    private const string RolesClientId = "RecallApp";

    /// <summary>
    /// Инициирует вход через Keycloak (redirect на хостовую страницу).
    /// При уже открытой сессии повторный вход не начинается: сразу redirect на returnUrl.
    /// </summary>
    /// <param name="returnUrl">Локальный путь возврата после входа. Внешние URL отклоняются.</param>
    [HttpGet("/login")]
    public IActionResult Login([FromQuery] string? returnUrl = "/")
    {
        var target = returnUrl is not null && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
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
        if (User.Identity?.IsAuthenticated != true)
            return Redirect("/");

        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>Состояние текущей сессии: авторизация, личность из токена, client-роли RecallApp. Поля по белому списку</summary>
    [HttpGet("/session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // Состояние сессии не кэшируется: bfcache/back-навигация иначе отдавали бы устаревшее состояние.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Session()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new { isAuthenticated = false });

        // Роли приложения - client-роли RecallApp в access-токене (resource_access), ADR 0024.
        // В ID-токене/principal их нет, поэтому достаём из сохранённого access-токена.
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        var roles = ExtractRecallAppRoles(accessToken);

        // Внутренние идентификаторы (sid, session_state и пр.) наружу не отдаём.
        return Ok(new
        {
            isAuthenticated = true,
            sub = User.FindFirst("sub")?.Value,
            name = User.Identity.Name,
            email = User.FindFirst("email")?.Value,
            roles,
        });
    }

    private static string[] ExtractRecallAppRoles(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return [];

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
        if (!jwt.TryGetPayloadValue<JsonElement>("resource_access", out var resourceAccess)
            || !resourceAccess.TryGetProperty(RolesClientId, out var client)
            || !client.TryGetProperty("roles", out var rolesElement)
            || rolesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return rolesElement.EnumerateArray()
            .Select(e => e.GetString())
            .Where(r => r is not null)
            .ToArray()!;
    }
}
