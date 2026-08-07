using System.Runtime.Serialization;
using EDMS1.CommandLog.Commands;
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
public class RetryTests : IDisposable
{
	// per-test конфигурация
	private readonly TestWebApplicationFactory _factory = new();

	public void Dispose()
	{
		_factory.Dispose();
	}

	[Fact]
	public async Task LogRetryCommandAsync_FailedEntry_MarksEntryAsRetry()
	{
		Guid commandLogId;

		// Arrange
		using (var scope = _factory.Services.CreateScope())
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
		using (var scope = _factory.Services.CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.LogRetryCommandAsync(commandLogId);
		}


		// Assert
		using (var scope = _factory.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var entry = await dbContext.FindAsync<CommandLogEntry>([commandLogId],
				TestContext.Current.CancellationToken);

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
			using var scope = _factory.Services.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.LogRetryCommandAsync(commandLogId);
		});
	}

	[Fact]
	public async Task RetryCommandNowAsync_FailedEntry_RetriesSuccessfully()
	{
		Guid commandLogId;
		Guid todoId;
		string command;

		// Arrange
		using (var scope = _factory.Services.CreateScope())
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
			command = entry.Command;
		}

		// Act
		using (var scope = _factory.Services.CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandNowAsync(commandLogId, TestContext.Current.CancellationToken);
		}

		// Assert
		using (var scope = _factory.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var retryEntry = await dbContext.Set<CommandLogEntry>()
				.SingleAsync(e => e.Command == command && e.Status == CommandStatus.RetryProcessed.ToString(),
					TestContext.Current.CancellationToken);
			Assert.Equal(CommandStatus.RetryProcessed.ToString(), retryEntry.Status);

			Assert.Equal(1, await dbContext.Set<CommandLogEntry>()
				.CountAsync(e => e.Status == CommandStatus.Successful.ToString(),
					TestContext.Current.CancellationToken));

			var todo = await dbContext.FindAsync<Todo>([todoId], TestContext.Current.CancellationToken);
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
			using var scope = _factory.Services.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandNowAsync(commandLogId, TestContext.Current.CancellationToken);
		});
	}

	[Fact]
	public async Task RetryCommandNowAsync_NotACommand_ThrowsSerializationException()
	{
		Guid commandLogId;

		// Arrange
		using (var scope = _factory.Services.CreateScope())
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
			await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

			commandLogId = entry.CommandLogId;
		}

		// Act & Assert
		await Assert.ThrowsAsync<SerializationException>(async () =>
		{
			using var scope = _factory.Services.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandNowAsync(commandLogId, TestContext.Current.CancellationToken);
		});
	}

	[Fact]
	public async Task RetryCommandsAsync_FailedEntriesAmount1000_Retries1000Successfully()
	{
		// Arrange
		using (var scope = _factory.Services.CreateScope())
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
				"Couldn't connect to the database.",
				null);
		}

		// Act
		using (var scope = _factory.Services.CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(TestContext.Current.CancellationToken);
		}

		// Assert
		using (var scope = _factory.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(1000,
				await dbContext.Set<CommandLogEntry>()
					.CountAsync(e => e.Status == CommandStatus.RetryProcessed.ToString(),
						TestContext.Current.CancellationToken));
			Assert.Equal(1000,
				await dbContext.Set<CommandLogEntry>()
					.CountAsync(e => e.Status == CommandStatus.Successful.ToString(),
						TestContext.Current.CancellationToken));
			Assert.Equal(1000, await dbContext.Set<Todo>().CountAsync(TestContext.Current.CancellationToken));
		}
	}

	[Fact]
	public async Task RetryCommandsAsync_FailedAndNotACommandEntriesAmount1000_RetriesFailedSuccessfully()
	{
		// Arrange
		using (var scope = _factory.Services.CreateScope())
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
				"Couldn't connect to the database.",
				null)).ToArray();

			foreach (var entry in entries.Take(500)) entry.Command = "null";

			await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
		}

		// Act
		using (var scope = _factory.Services.CreateScope())
		{
			var svc = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
			await svc.RetryCommandsAsync(TestContext.Current.CancellationToken);
		}

		// Assert
		using (var scope = _factory.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			Assert.Equal(500,
				await dbContext.Set<CommandLogEntry>().CountAsync(e => e.Status == CommandStatus.Retry.ToString(),
					TestContext.Current.CancellationToken));
			Assert.Equal(500, await dbContext.Set<CommandLogEntry>().CountAsync(
				e => e.Status == CommandStatus.RetryProcessed.ToString(),
				TestContext.Current.CancellationToken));
			Assert.Equal(500,
				await dbContext.Set<CommandLogEntry>()
					.CountAsync(e => e.Status == CommandStatus.Successful.ToString(),
						TestContext.Current.CancellationToken));
		}
	}
}