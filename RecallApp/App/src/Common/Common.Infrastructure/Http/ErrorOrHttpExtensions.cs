using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Infrastructure.Http;

/// <summary>
/// Предоставляет маппинг ErrorOr-ошибок в ProblemDetails-ответы.
/// </summary>
/// <remarks>
/// <para>Тип ошибки определяет HTTP-статус (ErrorType -> HTTP-статус).</para>
/// <para>Код ошибки всегда в <c>title</c> (клиент читает его оттуда, ADR 0031, ADR 0038),
/// описание - в <c>detail</c>, поле - в extension-поле <c>target</c>.</para>
/// </remarks>
public static class ErrorOrHttpExtensions
{
    /// <summary>
    /// Преобразует одну ошибку в ProblemDetails-ответ.
    /// </summary>
    /// <param name="error">Ошибка.</param>
    /// <returns>HTTP-ответ с кодом ошибки в <c>title</c>.</returns>
    public static IResult ToProblem(this Error error) => Single(error);

    /// <summary>
    /// Преобразует список ошибок в ответ.
    /// </summary>
    /// <remarks>
    /// Ошибки валидации собираются в ValidationProblem (errors по полям); иначе берётся первая.
    /// </remarks>
    /// <param name="errors">Ошибки.</param>
    /// <returns>HTTP-ответ.</returns>
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 1)
            return Single(errors[0]);

        return Results.ValidationProblem(errors.ToDictionary(
            e => Target(e),
            e => new[] { e.Description }));
    }

    private static IResult Single(Error error) => error.Type switch
    {
        ErrorType.Validation => Results.Problem(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = error.Code,
            Detail = error.Description,
            Extensions = { ["target"] = Target(error) },
        }),
        ErrorType.NotFound => Problem(error, StatusCodes.Status404NotFound),
        ErrorType.Conflict => Problem(error, StatusCodes.Status409Conflict),
        ErrorType.Forbidden => Problem(error, StatusCodes.Status403Forbidden),
        ErrorType.Unauthorized => Problem(error, StatusCodes.Status401Unauthorized),
        _ => Problem(error, StatusCodes.Status500InternalServerError),
    };

    private static IResult Problem(Error error, int statusCode) => Results.Problem(
        detail: error.Description,
        statusCode: statusCode,
        title: error.Code,
        extensions: new Dictionary<string, object?> { ["target"] = Target(error) });

    private static string Target(Error error) =>
        error.Metadata is not null && error.Metadata.TryGetValue("target", out var target)
            ? target.ToString() ?? error.Code
            : error.Code;
}
