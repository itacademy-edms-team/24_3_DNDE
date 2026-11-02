using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api;

/// <summary>
/// Маппинг ErrorOr-ошибок в ProblemDetails-ответы (ErrorType → HTTP-статус).
/// </summary>
public static class ErrorOrExtensions
{
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
        ErrorType.NotFound => Results.Problem(error.Description, error.Code, StatusCodes.Status404NotFound),
        ErrorType.Conflict => Results.Problem(error.Description, error.Code, StatusCodes.Status409Conflict),
        ErrorType.Forbidden => Results.Problem(error.Description, error.Code, StatusCodes.Status403Forbidden),
        ErrorType.Unauthorized => Results.Problem(error.Description, error.Code, StatusCodes.Status401Unauthorized),
        _ => Results.Problem(error.Description, error.Code, StatusCodes.Status500InternalServerError),
    };

    private static string Target(Error error) =>
        error.Metadata is not null && error.Metadata.TryGetValue("target", out var target)
            ? target.ToString() ?? error.Code
            : error.Code;
}
