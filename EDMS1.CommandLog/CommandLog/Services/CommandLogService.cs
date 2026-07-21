using System.Runtime.Serialization;
using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Db;
using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EDMS1.CommandLog.Services;

/// <summary>
/// Сервис логирования вызовов команд.
/// </summary>
public class CommandLogService : ICommandLogService
{
    private readonly ICommandTypes _commandTypes;
    private readonly IMediator _mediator;
    private readonly CommandLogDbContext _commandLogDbContext;
    private readonly ILogger<CommandLogService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLogService"/> class.
    /// </summary>
    public CommandLogService(DbContext commandLogDbContext, ICommandTypes commandTypes, ILogger<CommandLogService> logger, IMediator mediator)
    {
        _commandTypes = commandTypes;
        _logger = logger;
        _mediator = mediator;
        _commandLogDbContext = (CommandLogDbContext)commandLogDbContext;
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

        _commandLogDbContext.CommandLogs.Add(commandLogEntry);

        await _commandLogDbContext.SaveChangesAsync();
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

        _commandLogDbContext.CommandLogs.Add(commandLogEntry);

        await _commandLogDbContext.SaveChangesAsync();
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
        var commandLogs = await _commandLogDbContext.CommandLogs.AsNoTracking()
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
        var command = JsonConvert.DeserializeObject(cmdLogEntry.Command, _commandTypes[cmdLogEntry.CommandName])
            ?? throw new SerializationException(
                $"Cannot deserialize retry command {cmdLogEntry.Command} to type {_commandTypes[cmdLogEntry.CommandName]}.");
        
        await UpdateLogStatusAsync(cmdLogEntry.CommandLogId, CommandStatus.RetryProcessed, cancel);

        await _mediator.Send(command, cancel);
    }

    private async Task<CommandLogEntry> UpdateLogStatusAsync(Guid id, CommandStatus status, CancellationToken cancel)
    {
        var commandLog = await _commandLogDbContext.CommandLogs.FindAsync([id], cancel)
            ?? throw new EntityNotFoundException(nameof(CommandLogEntry), nameof(CommandLogEntry.CommandLogId), id);

        commandLog.Status = status.ToString();

        await _commandLogDbContext.SaveChangesAsync(cancel);

        return commandLog;
    }
}
