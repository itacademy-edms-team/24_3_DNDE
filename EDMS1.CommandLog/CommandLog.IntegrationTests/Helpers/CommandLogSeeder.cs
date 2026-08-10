using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;
using Microsoft.EntityFrameworkCore;

namespace EDMS1.CommandLog.IntegrationTests.Helpers;

public static class CommandLogSeeder
{
	public static async Task<CommandLogEntry> SeedAsync(DbContext db, ICommand command, Guid transactionId,
		CommandStatus status, string? failureMessage, string? comment)
	{
		var entry = CommandLogEntry.Create(command, transactionId, status, failureMessage, comment);

		db.Add(entry);
		await db.SaveChangesAsync();

		return entry;
	}

	public static async Task<IEnumerable<CommandLogEntry>> SeedManyAsync(DbContext db, IEnumerable<ICommand> commands,
		Guid transactionId,
		CommandStatus status, string? failureMessage, string? comment)
	{
		var entries = new List<CommandLogEntry>();

		foreach (var command in commands)
		{
			var entry = CommandLogEntry.Create(command, transactionId, status, failureMessage, comment);
			db.Add(entry);

			entries.Add(entry);
		}

		await db.SaveChangesAsync();

		return entries;
	}
}