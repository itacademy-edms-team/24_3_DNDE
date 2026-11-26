using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Data;

/// <summary>
/// Композиция http-логирования в БД.
/// Рантайм-запись идёт через NLog DatabaseTarget (плагин HttpLogging), здесь только DDL.
/// </summary>
public static class LoggingModule
{
    /// <summary>
    /// Применяет миграции под пользователем-владельцем схемы logging.
    /// </summary>
    public static async Task MigrateLogging(string connectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseNpgsql(connectionString, pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "logging"))
            .Options;

        await using var db = new LoggingDbContext(options);
        await db.Database.MigrateAsync(cancellationToken);
    }
}
