using Microsoft.EntityFrameworkCore;
using Sets.Domain.Sets;

namespace Sets.Infrastructure.Data;

/// <summary>
/// DbContext модуля Sets.
/// </summary>
internal sealed class SetsDbContext(DbContextOptions<SetsDbContext> options) : DbContext(options)
{
    public DbSet<Set> Sets => Set<Set>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sets");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SetsDbContext).Assembly);
    }
}
