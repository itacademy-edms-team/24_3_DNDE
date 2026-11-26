using HttpLogging;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Data;

/// <summary>
/// Системный DbContext логирования.
/// </summary>
public class LoggingDbContext(DbContextOptions<LoggingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseDatabaseLogging("logging");
        base.OnModelCreating(modelBuilder);
    }
}
