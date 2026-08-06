using System.Runtime.Serialization;
using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Resolvers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EDMS1.CommandLog.Services;

/// <summary>
/// <para>
/// Сервис логирования вызовов команд.
/// </para>
/// <remarks>
/// Пример реализации взят из <see href="https://github.com/dotnet/eShop/tree/main/src/IntegrationEventLogEF">eShop</see>
/// </remarks>
/// </summary>
internal sealed class CommandLogService<TContext> : ICommandLogService
    where TContext : DbContext
{
    private readonly CommandTypeResolver _typeResolver;
    private readonly IMediator _mediator;
    private readonly TContext _dbContext;
    private readonly ILogger<CommandLogService<TContext>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLogService{TContext}"/> class.
    /// </summary>
    public CommandLogService(TContext dbContext, CommandTypeResolver typeResolver, ILogger<CommandLogService<TContext>> logger, IMediator mediator)
    {
        _typeResolver = typeResolver;
        _logger = logger;
        _mediator = mediator;
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task LogCommandAsync(ICommand command, CommandResult result, Guid transactionId)
    {
        var commandLogEntry = CommandLogEntry.Create(
            command,
            transactionId,
            result.Status,
            null,
            result.Comment);

        _dbContext.Set<CommandLogEntry>().Add(commandLogEntry);

        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task LogFailedCommandAsync(ICommand command, Guid transactionId, string message)
    {
        var commandLogEntry = CommandLogEntry.Create(
            command,
            transactionId,
            CommandStatus.Failed,
            message,
            null);

        _dbContext.Set<CommandLogEntry>().Add(commandLogEntry);

        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public Task LogRetryCommandAsync(Guid id)
    {
        return UpdateLogStatusAsync(id, CommandStatus.Retry, CancellationToken.None);
    }

    /// <inheritdoc/>
    public async Task RetryCommandNowAsync(Guid id, CancellationToken cancel)
    {
        var commandLog = await UpdateLogStatusAsync(id, CommandStatus.Retry, cancel);

        await RetryCommandAsync(commandLog, cancel);
    }

    /// <inheritdoc/>
    public async Task RetryCommandsAsync(CancellationToken cancel)
    {
        var commandLogs = await _dbContext.Set<CommandLogEntry>().AsNoTracking()
            .Where(x => x.Status == CommandStatus.Retry.ToString())
            .OrderBy(x => x.CreationTime)
            .Take(1000)
            .ToListAsync(cancel);

        foreach (var commandLog in commandLogs)
        {
            try
            {
                await RetryCommandAsync(commandLog, cancel);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Retrying of commands was cancelled.");

                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Retrying of commands was failed.");
            }
        }
    }

    private async Task RetryCommandAsync(CommandLogEntry cmdLogEntry, CancellationToken cancel)
    {
        var command = JsonConvert.DeserializeObject(cmdLogEntry.Command, _typeResolver.Resolve(cmdLogEntry.CommandName))
            ?? throw new SerializationException(
                $"Cannot deserialize retry command {cmdLogEntry.Command} to type {cmdLogEntry.CommandName}.");
        
        await UpdateLogStatusAsync(cmdLogEntry.CommandLogId, CommandStatus.RetryProcessed, cancel);

        await _mediator.Send(command, cancel);
    }

    private async Task<CommandLogEntry> UpdateLogStatusAsync(Guid id, CommandStatus status, CancellationToken cancel)
    {
        var commandLog = await _dbContext.Set<CommandLogEntry>().FindAsync([id], cancel)
            ?? throw new EntityNotFoundException(nameof(CommandLogEntry), nameof(CommandLogEntry.CommandLogId), id);

        commandLog.Status = status.ToString();

        await _dbContext.SaveChangesAsync(cancel);

        return commandLog;
    }
}
