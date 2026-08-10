using System.Runtime.Serialization;
using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.IntegrationTests.Helpers;
using EDMS1.CommandLog.IntegrationTests.Infrastructure;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplicationExample.Commands;
using WebApplicationExample.Database;
using WebApplicationExample.Models;

namespace EDMS1.CommandLog.IntegrationTests;

/// <summary>
/// Набор тестов для проверки работоспособности ретрая команд.
/// </summary>
public class RetryTests : IntegrationTestBase
{
	[Fact]
	public async Task LogRetryCommandAsync_FailedEntry_MarksEntryAsRetry()
	{
		Guid commandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				new AddTodo
				{
					Title = $"{Guid.NewGuid()}",
					Description = $"{Guid.NewGuid()}"
				},
				Guid.NewGuid(),
				CommandStatus.Failed,
				"Couldn't connect to the database.",
				null);

			commandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.LogRetryCommandAsync(commandLogId);
		}


		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var entry = await dbContext.FindAsync<CommandLogEntry>([commandLogId],
				Ct);

			Assert.NotNull(entry);
			Assert.Equal(CommandStatus.Retry.ToString(), entry.Status);
		}
	}

	[Fact]
	public async Task LogRetryCommandAsync_UnexistingEntry_ThrowsEntityNotFoundException()
	{
		// Arrange
		var commandLogId = Guid.NewGuid();

		// Act & Assert
		await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
		{
			using var scope = CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.LogRetryCommandAsync(commandLogId);
		});
	}

	[Fact]
	public async Task RetryCommandNowAsync_FailedEntry_RetriesSuccessfully()
	{
		Guid commandLogId;
		Guid todoId;

		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todo = new AddTodo
			{
				Title = $"{Guid.NewGuid()}",
				Description = $"{Guid.NewGuid()}"
			};

			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				todo,
				Guid.NewGuid(),
				CommandStatus.Failed,
				"Couldn't connect to the database.",
				null);

			commandLogId = entry.CommandLogId;
			todoId = todo.Id;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandNowAsync(commandLogId, Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var retryEntry = await dbContext.FindAsync<CommandLogEntry>([commandLogId], Ct);
			Assert.NotNull(retryEntry);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), retryEntry.Status);

			Assert.Equal(1, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(),
					Ct));

			var todo = await dbContext.FindAsync<Todo>([todoId], Ct);
			Assert.NotNull(todo);
		}
	}

	[Fact]
	public async Task RetryCommandNowAsync_UnexistingEntry_ThrowsEntityNotFoundException()
	{
		// Arrange
		var commandLogId = Guid.NewGuid();

		// Act & Assert
		await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
		{
			using var scope = CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandNowAsync(commandLogId, Ct);
		});
	}

	[Fact]
	public async Task RetryCommandNowAsync_NotACommand_ThrowsSerializationException()
	{
		Guid commandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				new AddTodo
				{
					Title = $"{Guid.NewGuid()}",
					Description = $"{Guid.NewGuid()}"
				},
				Guid.NewGuid(),
				CommandStatus.Failed,
				"Couldn't connect to the database.",
				null);

			entry.Command = "null";
			await dbContext.SaveChangesAsync(Ct);

			commandLogId = entry.CommandLogId;
		}

		// Act & Assert
		await Assert.ThrowsAsync<SerializationException>(async () =>
		{
			using var scope = CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandNowAsync(commandLogId, Ct);
		});
	}

	[Fact]
	public async Task RetryCommandsAsync_1000RetryEntries_ProcessesAllAndLogsSuccess()
	{
		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todos = new List<AddTodo>();
			for (var i = 0; i < 1000; i++)
				todos.Add(new AddTodo
				{
					Title = $"{Guid.NewGuid()}",
					Description = $"{Guid.NewGuid()}"
				});

			await CommandLogSeeder.SeedManyAsync(dbContext,
				todos,
				Guid.NewGuid(),
				CommandStatus.Retry,
				null,
				null);
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(1000,
				await dbContext.Set<CommandLogEntry>()
					.CountAsync(e => e.Status == CommandStatus.RetryProcessed.ToString(),
						Ct));
			Assert.Equal(1000,
				await dbContext.Set<CommandLogEntry>()
					.CountAsync(e => e.Status == CommandStatus.Successful.ToString(),
						Ct));
			Assert.Equal(1000, await dbContext.Set<Todo>().CountAsync(Ct));
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_MixedValidAndCorruptedEntries_ProcessesValidLeavesCorruptedAsRetry()
	{
		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todos = new List<AddTodo>();
			for (var i = 0; i < 1000; i++)
				todos.Add(new AddTodo
				{
					Title = $"{Guid.NewGuid()}",
					Description = $"{Guid.NewGuid()}"
				});

			var entries = (await CommandLogSeeder.SeedManyAsync(dbContext,
				todos,
				Guid.NewGuid(),
				CommandStatus.Retry,
				null,
				null)).ToArray();

			foreach (var entry in entries.Take(500)) entry.Command = "null";

			await dbContext.SaveChangesAsync(Ct);
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(500,
				await dbContext.Set<CommandLogEntry>().CountAsync(e => e.Status == CommandStatus.Retry.ToString(),
					Ct));
			Assert.Equal(500, await dbContext.Set<CommandLogEntry>().CountAsync(
				e => e.Status == CommandStatus.RetryProcessed.ToString(),
				Ct));
			Assert.Equal(500,
				await dbContext.Set<CommandLogEntry>()
					.CountAsync(e => e.Status == CommandStatus.Successful.ToString(),
						Ct));
		}
	}

	[Theory]
	[InlineData(CommandStatus.Successful)]
	[InlineData(CommandStatus.Failed)]
	[InlineData(CommandStatus.RetryProcessed)]
	public async Task RetryCommandsAsync_NonRetryStatus_NotProcessed(CommandStatus status)
	{
		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todos = new List<AddTodo>();
			for (var i = 0; i < 1000; i++)
				todos.Add(new AddTodo
				{
					Title = $"{Guid.NewGuid()}",
					Description = $"{Guid.NewGuid()}"
				});

			await CommandLogSeeder.SeedManyAsync(dbContext,
				todos,
				Guid.NewGuid(),
				status,
				null,
				null);
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(0, await dbContext.Set<Todo>().CountAsync(Ct));
			Assert.Equal(1000,
				await dbContext.Set<CommandLogEntry>().CountAsync(e => e.Status == status.ToString(),
					Ct));
		}
	}

	[Fact]
	public async Task
		RetryCommandsAsync_CreateTodoCommand_SuccessfullyProcessed()
	{
		Guid todoId;

		// Arrange & Act
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addTodoCommand = new AddTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addTodoCommand, Ct);

			todoId = addTodoCommand.Id;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todo = await dbContext.Set<Todo>().FindAsync([todoId], Ct);
			Assert.Equal(1, await dbContext.Set<Todo>().CountAsync(Ct));
			Assert.NotNull(todo);

			var entry = await dbContext.Set<CommandLogEntry>().SingleAsync(Ct);
			Assert.Equal(1, await dbContext.Set<CommandLogEntry>().CountAsync(Ct));
			Assert.Equal(CommandStatus.Successful.ToString(), entry.Status);
		}
	}

	[Fact]
	public async Task
		RetryCommandsAsync_RetryOfAlreadyCreatedTodo_ReprocessesButDoesNotDuplicate()
	{
		Todo todoBeforeRetry;
		Guid secondAddTodoCommandLogId;
		Guid firstAddTodoCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addTodoCommand = new AddTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addTodoCommand, Ct);

			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			todoBeforeRetry = await dbContext.Set<Todo>().SingleAsync((t) => t.Id == addTodoCommand.Id, Ct);

			var secondEntry = await CommandLogSeeder.SeedAsync(dbContext,
				addTodoCommand, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			secondAddTodoCommandLogId = secondEntry.CommandLogId;

			var firstEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(e => e.CommandLogId != secondAddTodoCommandLogId, Ct);
			firstAddTodoCommandLogId = firstEntry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todoAfterRetry = await dbContext.Set<Todo>().FindAsync([todoBeforeRetry.Id], Ct);
			Assert.NotNull(todoAfterRetry);
			Assert.Equal(1, await dbContext.Set<Todo>().CountAsync(Ct));

			Assert.Equal(3, await dbContext.Set<CommandLogEntry>().CountAsync(Ct));

			var firstAddTodoEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(e => e.CommandLogId == firstAddTodoCommandLogId, Ct);
			Assert.Equal(CommandStatus.Successful.ToString(), firstAddTodoEntry.Status);

			var secondAddTodoEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(e => e.CommandLogId == secondAddTodoCommandLogId, Ct);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), secondAddTodoEntry.Status);

			var failedAddTodoEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(
					e => e.CommandLogId != firstAddTodoCommandLogId
					     && e.CommandLogId != secondAddTodoCommandLogId, Ct);
			Assert.Equal(CommandStatus.Failed.ToString(), failedAddTodoEntry.Status);
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_RetryOfUpdateTodo_UpdatesTargetEntity()
	{
		Todo todoBeforeEdit;
		Guid retryCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addTodoCommand = new AddTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addTodoCommand, Ct);

			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			todoBeforeEdit = await dbContext.Set<Todo>().SingleAsync((t) => t.Id == addTodoCommand.Id, Ct);

			var updateTodoCommand = new UpdateTodo
				{ Id = addTodoCommand.Id, Title = "Updated Todo1", Description = "Updated Todo1 description" };
			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				updateTodoCommand, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			retryCommandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todoAfterEdit = await dbContext.Set<Todo>().SingleAsync(t => t.Id == todoBeforeEdit.Id, Ct);
			Assert.NotEqual(todoBeforeEdit.Title, todoAfterEdit.Title);
			Assert.NotEqual(todoBeforeEdit.Description, todoAfterEdit.Description);
			Assert.True(todoAfterEdit.UpdatedAt > todoBeforeEdit.UpdatedAt);
			Assert.Equal(todoBeforeEdit.CreatedAt, todoAfterEdit.CreatedAt);

			var processedEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(e => e.CommandLogId == retryCommandLogId, Ct);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), processedEntry.Status);

			var successfulEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(
					e => e.CommandName == nameof(UpdateTodo) && e.Status == CommandStatus.Successful.ToString(), Ct);
			Assert.Equal(CommandStatus.Successful.ToString(), successfulEntry.Status);
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_RetryOfUpdateTodoWhenEntityIsDeleted_ReprocessesButDoesNotUpdate()
	{
		Guid updateTodoCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addTodoCommand = new AddTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addTodoCommand, Ct);

			var deleteTodoCommand = new DeleteTodo { Id = addTodoCommand.Id };
			await mediator.Send(deleteTodoCommand, Ct);

			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var updateTodoCommand = new UpdateTodo
				{ Id = addTodoCommand.Id, Title = "Updated Todo1", Description = "Updated Todo1 description" };
			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				updateTodoCommand, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			updateTodoCommandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(0, await dbContext.Set<Todo>().CountAsync(Ct));

			Assert.Equal(4, await dbContext.Set<CommandLogEntry>().CountAsync(Ct));

			var updateEntry = await dbContext.Set<CommandLogEntry>().FindAsync([updateTodoCommandLogId], Ct);
			Assert.NotNull(updateEntry);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), updateEntry.Status);

			Assert.Equal(2, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(), Ct));
			Assert.Equal(1, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Failed.ToString(), Ct));
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_RetryOfDeleteTodo_DeletesTargetEntity()
	{
		Guid retryDeleteEntryCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addTodoCommand = new AddTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addTodoCommand, Ct);

			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var deleteTodoCommand = new DeleteTodo { Id = addTodoCommand.Id };
			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				deleteTodoCommand, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			retryDeleteEntryCommandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(0, await dbContext.Set<Todo>().CountAsync(Ct));

			Assert.Equal(3, await dbContext.Set<CommandLogEntry>().CountAsync(Ct));

			var retryDeleteEntry = await dbContext.Set<CommandLogEntry>().FindAsync([retryDeleteEntryCommandLogId], Ct);
			Assert.NotNull(retryDeleteEntry);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), retryDeleteEntry.Status);

			Assert.Equal(2, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(), Ct));
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_RetryOfDeleteTodoWhenEntityIsDeleted_ReprocessesButDoesNotDelete()
	{
		Guid retryDeleteTodoCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addTodoCommand = new AddTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addTodoCommand, Ct);

			var deleteTodoCommand = new DeleteTodo { Id = addTodoCommand.Id };
			await mediator.Send(deleteTodoCommand, Ct);

			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				deleteTodoCommand, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			retryDeleteTodoCommandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(0, await dbContext.Set<Todo>().CountAsync(Ct));

			Assert.Equal(4, await dbContext.Set<CommandLogEntry>().CountAsync(Ct));

			var retryDeleteEntry = await dbContext.Set<CommandLogEntry>().FindAsync([retryDeleteTodoCommandLogId], Ct);
			Assert.NotNull(retryDeleteEntry);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), retryDeleteEntry.Status);

			Assert.Equal(2, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(), Ct));
			Assert.Equal(1, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Failed.ToString(), Ct));
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_RetryOfAddOrUpdateTodoWhenNotExists_CreatesEntity()
	{
		Guid todoId;
		Guid addOrUpdateCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var command = new AddOrUpdateTodo { Title = "Todo1", Description = "Todo1 description" };
			todoId = command.Id;

			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				command, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			addOrUpdateCommandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var todo = await dbContext.Set<Todo>().FindAsync([todoId], Ct);
			Assert.NotNull(todo);
			Assert.Equal(1, await dbContext.Set<Todo>().CountAsync(Ct));

			var addOrUpdateEntry = await dbContext.FindAsync<CommandLogEntry>([addOrUpdateCommandLogId], Ct);
			Assert.NotNull(addOrUpdateEntry);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), addOrUpdateEntry.Status);

			Assert.Equal(1, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(), Ct));
			Assert.Equal(0, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Failed.ToString(), Ct));
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_RetryOfAddOrUpdateTodoWhenExists_UpdatesEntityWithoutDuplicate()
	{
		Todo todoBeforeRetry;
		Guid addOrUpdateCommandLogId;

		// Arrange
		using (var scope = CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			var addCommand = new AddOrUpdateTodo { Title = "Todo1", Description = "Todo1 description" };
			await mediator.Send(addCommand, Ct);

			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			todoBeforeRetry = await dbContext.Set<Todo>().SingleAsync(t => t.Id == addCommand.Id, Ct);

			var updateCommand = new AddOrUpdateTodo
				{ Id = addCommand.Id, Title = "Same Todo1", Description = "Same Todo1 description" };
			var entry = await CommandLogSeeder.SeedAsync(dbContext,
				updateCommand, Guid.NewGuid(),
				CommandStatus.Retry, null, null);
			addOrUpdateCommandLogId = entry.CommandLogId;
		}

		// Act
		using (var scope = CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(Ct);
		}

		// Assert
		using (var scope = CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(1, await dbContext.Set<Todo>().CountAsync(Ct));
			var todoAfterRetry = await dbContext.Set<Todo>().SingleAsync(t => t.Id == todoBeforeRetry.Id, Ct);
			Assert.NotEqual(todoBeforeRetry.Title, todoAfterRetry.Title);
			Assert.NotEqual(todoBeforeRetry.Description, todoAfterRetry.Description);
			Assert.Equal("Same Todo1", todoAfterRetry.Title);
			Assert.Equal("Same Todo1 description", todoAfterRetry.Description);

			var addOrUpdateEntry = await dbContext.FindAsync<CommandLogEntry>([addOrUpdateCommandLogId], Ct);
			Assert.NotNull(addOrUpdateEntry);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), addOrUpdateEntry.Status);

			Assert.Equal(3, await dbContext.Set<CommandLogEntry>().CountAsync(Ct));
			Assert.Equal(2, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(), Ct));
			Assert.Equal(0, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Failed.ToString(), Ct));
		}
	}
}