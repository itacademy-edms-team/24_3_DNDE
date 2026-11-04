using Microsoft.AspNetCore.Mvc;

namespace Host.Infrastructure;

/// <summary>
/// Ответы HTTP 401 в формате problem+json для запросов к /api/**.
/// </summary>
public static class ApiProblem
{
    /// <summary>Сессии нет или она уже недействительна на момент запроса.</summary>
    public const string NotAuthenticated = "not_authenticated";

    /// <summary>Сессия была, но Keycloak отказал в обновлении токена: сессия завершена.</summary>
    public const string SessionExpired = "session_expired";

    /// <summary>Мутация /api/** без заголовка X-CSRF: кросс-сайт-запрос отклонён.</summary>
    public const string CsrfFailed = "csrf_failed";

    /// <summary>
    /// 401 для запросов из fetch: SPA не может проследовать за 302 в Keycloak,
    /// поэтому вместо редиректа клиент получает код причины отказа.
    /// </summary>
    public static Task WriteUnauthorizedAsync(HttpContext context, string code) =>
        WriteAsync(context, code, StatusCodes.Status401Unauthorized, Describe(code));

    /// <summary>403 для запросов, отклонённых на входе в /api/**.</summary>
    public static Task WriteForbiddenAsync(HttpContext context, string code) =>
        WriteAsync(context, code, StatusCodes.Status403Forbidden, Describe(code));

    private static Task WriteAsync(HttpContext context, string code, int status, string detail)
    {
        // Статус выставляет вызывающий: IProblemDetailsService только сериализует тело
        // (в отличие от Results.Problem, который статус ставит сам).
        context.Response.StatusCode = status;

        return context
            .RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new ProblemDetails
                    {
                        Title = code,
                        Status = status,
                        Detail = detail,
                    },
                }
            )
            .AsTask();
    }

    /// <summary>Пояснение для разработчика: в интерфейсе используется текст из каталога кодов клиента.</summary>
    private static string Describe(string code) =>
        code switch
        {
            NotAuthenticated => "BFF session cookie is missing or no longer valid.",
            SessionExpired => "Token refresh was rejected by Keycloak; the session has been terminated.",
            CsrfFailed => "X-CSRF header is required for mutating requests under /api.",
            _ => code,
        };
}
