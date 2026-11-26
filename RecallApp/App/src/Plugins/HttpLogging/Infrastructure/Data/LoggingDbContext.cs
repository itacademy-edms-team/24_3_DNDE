using Microsoft.EntityFrameworkCore;

namespace HttpLogging.Infrastructure.Data;

/// <summary>
/// Контекст плагина http-логирования.
/// </summary>
internal sealed class LoggingDbContext(
    DbContextOptions<LoggingDbContext> options,
    HttpLoggingOptions loggingOptions) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseDatabaseLogging(loggingOptions.Schema);
        base.OnModelCreating(modelBuilder);
    }
}
