using Microsoft.AspNetCore.Http;

namespace Host.Infrastructure;

/// <summary>
/// Ответы 401 в формате problem+json для запросов к /api/**.
/// SPA не сможет проследовать за 302 в Keycloak, поэтому вместо редиректа
/// клиент получает код причины отказа.
/// </summary>
public static class ApiProblem
{
    /// <summary>Сессии нет или она уже недействительна на момент запроса.</summary>
    public const string NotAuthenticated = "not_authenticated";

    /// <summary>Сессия была, но Keycloak отказал в обновлении токена: сессия завершена.</summary>
    public const string SessionExpired = "session_expired";

    /// <summary>401 application/problem+json с машиночитаемым кодом причины отказа.</summary>
    public static Task WriteUnauthorizedAsync(HttpResponse response, string code)
    {
        response.StatusCode = StatusCodes.Status401Unauthorized;
        return response.WriteAsJsonAsync(
            new { type = "about:blank", title = "Unauthorized", status = 401, code },
            options: null,
            contentType: "application/problem+json"
        );
    }
}
