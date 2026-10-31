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

    /// <summary>Мутация /api/** без заголовка X-CSRF: кросс-сайт-запрос отклонён.</summary>
    public const string CsrfFailed = "csrf_failed";

    /// <summary>401 application/problem+json с машиночитаемым кодом причины отказа.</summary>
    public static Task WriteUnauthorizedAsync(HttpResponse response, string code)
    {
        response.StatusCode = StatusCodes.Status401Unauthorized;
        return WriteAsync(response, "Unauthorized", 401, code);
    }

    /// <summary>403 application/problem+json с машиночитаемым кодом причины отказа.</summary>
    public static Task WriteForbiddenAsync(HttpResponse response, string code)
    {
        response.StatusCode = StatusCodes.Status403Forbidden;
        return WriteAsync(response, "Forbidden", 403, code);
    }

    private static Task WriteAsync(HttpResponse response, string title, int status, string code) =>
        response.WriteAsJsonAsync(
            new { type = "about:blank", title, status, code },
            options: null,
            contentType: "application/problem+json"
        );
}
