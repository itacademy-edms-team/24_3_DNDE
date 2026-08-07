using System.Runtime.Serialization;
using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.IntegrationTests.Helpers;
using EDMS1.CommandLog.IntegrationTests.Infrastructure;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Services;
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
}