using System.Runtime.Serialization;
using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Resolvers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
internal sealed class CommandLogService<TContext> : ICommandLogService, IDisposable, IAsyncDisposable
	where TContext : DbContext
{
	private readonly CommandTypeResolver _typeResolver;
	private readonly IMediator _mediator;
	private readonly TContext _appDbContext;
	private readonly ILogger<CommandLogService<TContext>> _logger;

	// Нужны для получения DbContext, используемого в журналировании
	private readonly IServiceScopeFactory _scopeFactory;
	private IServiceScope? _journalScope;

	private TContext CommandLogDbContext
	{
		get
		{
			_journalScope ??= _scopeFactory.CreateScope();
			return _journalScope.ServiceProvider.GetRequiredService<TContext>();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandLogService{TContext}"/> class.
	/// </summary>
	public CommandLogService(IServiceScopeFactory scopeFactory, TContext appDbContext,
		CommandTypeResolver typeResolver,
		ILogger<CommandLogService<TContext>> logger, IMediator mediator)
	{
		_typeResolver = typeResolver;
		_logger = logger;
		_mediator = mediator;
		_scopeFactory = scopeFactory;

		// Этот же dbContext используют обработчики команд.
		// Нужен для подготовки контекста между ретраями команд (см. блок finally в RetryCommandsAsync).
		_appDbContext = appDbContext;
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

		CommandLogDbContext.Set<CommandLogEntry>().Add(commandLogEntry);

		await CommandLogDbContext.SaveChangesAsync();
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

		CommandLogDbContext.Set<CommandLogEntry>().Add(commandLogEntry);

		await CommandLogDbContext.SaveChangesAsync();
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
		var commandLogs = await CommandLogDbContext.Set<CommandLogEntry>().AsNoTracking()
			.Where(x => x.Status == CommandStatus.Retry.ToString())
			.OrderBy(x => x.CreationTime)
			.Take(1000)
			.ToListAsync(cancel);

		foreach (var commandLog in commandLogs)
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
			finally
			{
				_appDbContext.ChangeTracker.Clear();
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
		var commandLog = await CommandLogDbContext.Set<CommandLogEntry>().FindAsync([id], cancel)
		                 ?? throw new EntityNotFoundException(nameof(CommandLogEntry),
			                 nameof(CommandLogEntry.CommandLogId), id);

		commandLog.Status = status.ToString();

		await CommandLogDbContext.SaveChangesAsync(cancel);

		return commandLog;
	}

	public void Dispose()
	{
		_journalScope?.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (_journalScope is IAsyncDisposable journalScopeAsyncDisposable)
			await journalScopeAsyncDisposable.DisposeAsync();
		else
			_journalScope?.Dispose();
	}
}