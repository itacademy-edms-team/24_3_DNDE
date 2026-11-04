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
    private const string RolesClientId = "RecallApp";

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
                roles = ExtractRecallAppRoles(accessToken),
                accessTokenExpiresAt = ExtractExpiry(accessToken),
            }
        );
    }

    /// <summary>Client-роли RecallApp из access-токена (resource_access, ADR 0024). В principal их нет.</summary>
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
