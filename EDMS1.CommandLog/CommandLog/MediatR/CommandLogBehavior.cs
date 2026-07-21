using EDMS1.CommandLog.Extensions;
using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;
using MediatR;
using Newtonsoft.Json;
using ValidationException = FluentValidation.ValidationException;

namespace EDMS1.CommandLog.Mediatr;

/// <summary>
/// Пайплайн для записи в CommandLog.
/// </summary>
/// <typeparam name="TRequest">Запрос медиатора.</typeparam>
/// <typeparam name="TResponse">Ответ медиатора.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandLogBehavior{TRequest, TResponse}"/> class.
/// </remarks>
public class CommandLogBehavior<TRequest, TResponse>(
    ICommandLogService commandLogService,
    ICommandContext commandContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICommand
{

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancel)
    {
        try
        {
            var response = await next(cancel);

            var commandResult = response as CommandResult ?? CommandResult.Successful;

            var suppressSuccessLog = request is ISuppressSuccessLogCommand;

            if (suppressSuccessLog == false)
            {
                await commandLogService.LogCommandAsync(request, commandResult, commandContext.TransactionId);
            }

            return response;
        }
        catch (ValidationException e)
        {
            var typeName = request.GetGenericTypeName();
            var errors = JsonConvert.SerializeObject(e.Errors);

            await commandLogService.LogFailedCommandAsync(
                request,
                commandContext.TransactionId,
                $"Validation errors - {typeName} - Command: {request} - Errors: {errors}");

            throw;
        }
        catch (Exception ex)
        {
            await commandLogService.LogFailedCommandAsync(
                request,
                commandContext.TransactionId,
                ex.ToString());

            throw;
        }
    }
}
