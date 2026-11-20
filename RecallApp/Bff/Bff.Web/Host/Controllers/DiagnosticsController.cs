using System.Text.Json;
using Host.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Host.Controllers;

/// <summary>
/// Контроллер диагностики сессии.
/// </summary>
[ApiController]
public class DiagnosticsController(IOptions<DiagnosticsOptions> diagnostics) : ControllerBase
{
    private readonly DiagnosticsOptions _diagnostics = diagnostics.Value;

    /// <summary>
    /// Снимок сессии для отладки: личность, роли приложения и срок жизни access-токена.
    /// </summary>
    /// <returns>404 при выключенной диагностике.</returns>
    [HttpGet("/debug/session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // Состояние сессии не кэшируется: bfcache/back-навигация иначе отдавали бы устаревшее состояние.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Session()
    {
        if (!_diagnostics.Session)
            return NotFound();

        if (User.Identity?.IsAuthenticated != true)
            return Ok(new { isAuthenticated = false });

        var accessToken = await HttpContext.GetTokenAsync("access_token");

        // Внутренние идентификаторы (sid, session_state и пр.) наружу не отдаём.
        return Ok(
            new
            {
                isAuthenticated = true,
                sub = User.FindFirst("sub")?.Value,
                name = User.Identity.Name,
                email = User.FindFirst("email")?.Value,
                // Теперь в id-токене есть плоский roles, по которому можно собрать роли пользователя.
                roles = User.FindAll("roles").Select(c => c.Value).ToArray(),
                accessTokenExpiresAt = ExtractExpiry(accessToken),
            }
        );
    }

    /// <summary>
    /// Текущие значения для access, refresh, id токенов,
    /// полученные после входа в keycloak,
    /// расшифрованные из base64 представления.
    /// </summary>
    /// <returns>
    /// Человекочитаемый список из json-списков для каждого jwt-токена для первых 2-х его частей
    /// в порядке: access, refresh, id.
    /// </returns>
    [HttpGet("/debug/keycloak/tokens")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> KeycloakTokens()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
        var idToken = await HttpContext.GetTokenAsync("id_token");

        if (accessToken is null)
        {
            return NotFound("access_token is null");
        }

        if (refreshToken is null)
        {
            return NotFound("refresh_token is null");
        }

        if (idToken is null)
        {
            return NotFound("id_token is null");
        }

        return Ok(new []
        {
            accessToken.Split(".")
                .Take(2)
                .Select(Base64UrlTextEncoder.Decode)
                .Select(System.Text.Encoding.UTF8.GetString)
                .Select(jsonString => JsonDocument.Parse(jsonString).RootElement),
            refreshToken.Split(".")
                .Take(2)
                .Select(Base64UrlTextEncoder.Decode)
                .Select(System.Text.Encoding.UTF8.GetString)
                .Select(jsonString => JsonDocument.Parse(jsonString).RootElement),
            idToken.Split(".")
                .Take(2)
                .Select(Base64UrlTextEncoder.Decode)
                .Select(System.Text.Encoding.UTF8.GetString)
                .Select(jsonString => JsonDocument.Parse(jsonString).RootElement),
        });
    }

    /// <summary>Момент истечения access-токена (claim exp) или null, если claim отсутствует.</summary>
    private static DateTimeOffset? ExtractExpiry(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return null;

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
        return jwt.TryGetPayloadValue<long>("exp", out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp)
            : null;
    }
}
