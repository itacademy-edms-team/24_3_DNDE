using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Infrastructure.Http;

/// <summary>
/// Маппинг ErrorOr-ошибок в ProblemDetails-ответы (ErrorType → HTTP-статус).
/// Код ошибки всегда в title (клиент читает его оттуда, ADR 0031/0038),
/// описание - в detail, поле - в extension-поле target.
/// </summary>
public static class ErrorOrHttpExtensions
{
    /// <summary>Единственная ошибка → один problem+json с кодом в title.</summary>
    public static IResult ToProblem(this Error error) => Single(error);

    /// <summary>Ошибки валидации → ValidationProblem (errors по полям), иначе первая ошибка.</summary>
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
