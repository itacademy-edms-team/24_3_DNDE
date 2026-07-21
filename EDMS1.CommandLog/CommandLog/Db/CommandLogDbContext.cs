using EDMS1.CommandLog.Extensions;
using EDMS1.CommandLog.Models;
using Microsoft.EntityFrameworkCore;

namespace EDMS1.CommandLog.Db;

/// <summary>
/// Контекст логирования команд.
/// </summary>
public class CommandLogDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLogDbContext"/> class.
    /// </summary>
    public CommandLogDbContext(DbContextOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Записи контекста логирования команд.
    /// </summary>
    public DbSet<CommandLogEntry> CommandLogs { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.UseCommandLog();
    }
}