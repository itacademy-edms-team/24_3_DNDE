namespace FinanceTrack.Finance.Web.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Sends appropriate HTTP response based on Result status and returns true if the result was handled (not Ok).
    /// Returns false if the result is Ok, allowing the caller to handle the success case.
    /// </summary>
    public static async Task<bool> SendResultIfNotOk<TRequest, TResponse, T>(
        this Endpoint<TRequest, TResponse> endpoint,
        Result<T> result,
        CancellationToken cancellationToken = default
    ) where TRequest : notnull
    {
        switch (result.Status)
        {
            case ResultStatus.NotFound:
                await endpoint.HttpContext.Response.SendNotFoundAsync(cancellationToken);
                return true;

            case ResultStatus.Forbidden:
                await endpoint.HttpContext.Response.SendForbiddenAsync(cancellationToken);
                return true;

            case ResultStatus.Invalid:
            case ResultStatus.Error:
                endpoint.ValidationFailures.Add(new FluentValidation.Results.ValidationFailure("", result.Errors.First()));
                await endpoint.HttpContext.Response.SendErrorsAsync(
                    endpoint.ValidationFailures, 
                    cancellation: cancellationToken
                );
                return true;

            case ResultStatus.Ok:
            default:
                return false;
        }
    }

    /// <summary>
    /// Sends appropriate HTTP response based on Result status and returns true if the result was handled (not Ok).
    /// Returns false if the result is Ok, allowing the caller to handle the success case.
    /// </summary>
    public static async Task<bool> SendResultIfNotOk<TRequest, TResponse>(
        this Endpoint<TRequest, TResponse> endpoint,
        Result result,
        CancellationToken cancellationToken = default
    ) where TRequest : notnull
    {
        switch (result.Status)
        {
            case ResultStatus.NotFound:
                await endpoint.HttpContext.Response.SendNotFoundAsync(cancellationToken);
                return true;

            case ResultStatus.Forbidden:
                await endpoint.HttpContext.Response.SendForbiddenAsync(cancellationToken);
                return true;

            case ResultStatus.Invalid:
            case ResultStatus.Error:
                endpoint.ValidationFailures.Add(new FluentValidation.Results.ValidationFailure("", result.Errors.First()));
                await endpoint.HttpContext.Response.SendErrorsAsync(
                    endpoint.ValidationFailures, 
                    cancellation: cancellationToken
                );
                return true;

            case ResultStatus.Ok:
            default:
                return false;
        }
    }

    /// <summary>
    /// Sends appropriate HTTP response based on Result status and returns true if the result was handled (not Ok).
    /// Returns false if the result is Ok, allowing the caller to handle the success case.
    /// </summary>
    public static async Task<bool> SendResultIfNotOk<TResponse, T>(
        this EndpointWithoutRequest<TResponse> endpoint,
        Result<T> result,
        CancellationToken cancellationToken = default
    )
    {
        switch (result.Status)
        {
            case ResultStatus.NotFound:
                await endpoint.HttpContext.Response.SendNotFoundAsync(cancellationToken);
                return true;

            case ResultStatus.Forbidden:
                await endpoint.HttpContext.Response.SendForbiddenAsync(cancellationToken);
                return true;

            case ResultStatus.Invalid:
            case ResultStatus.Error:
                endpoint.ValidationFailures.Add(new FluentValidation.Results.ValidationFailure("", result.Errors.First()));
                await endpoint.HttpContext.Response.SendErrorsAsync(
                    endpoint.ValidationFailures, 
                    cancellation: cancellationToken
                );
                return true;

            case ResultStatus.Ok:
            default:
                return false;
        }
    }

    /// <summary>
    /// Sends appropriate HTTP response based on Result status and returns true if the result was handled (not Ok).
    /// Returns false if the result is Ok, allowing the caller to handle the success case.
    /// </summary>
    public static async Task<bool> SendResultIfNotOk<TResponse>(
        this EndpointWithoutRequest<TResponse> endpoint,
        Result result,
        CancellationToken cancellationToken = default
    )
    {
        switch (result.Status)
        {
            case ResultStatus.NotFound:
                await endpoint.HttpContext.Response.SendNotFoundAsync(cancellationToken);
                return true;

            case ResultStatus.Forbidden:
                await endpoint.HttpContext.Response.SendForbiddenAsync(cancellationToken);
                return true;

            case ResultStatus.Invalid:
            case ResultStatus.Error:
                endpoint.ValidationFailures.Add(new FluentValidation.Results.ValidationFailure("", result.Errors.First()));
                await endpoint.HttpContext.Response.SendErrorsAsync(
                    endpoint.ValidationFailures, 
                    cancellation: cancellationToken
                );
                return true;

            case ResultStatus.Ok:
            default:
                return false;
        }
    }
}
