using Microsoft.EntityFrameworkCore;
using Sets.Domain.Elements;
using Sets.Domain.Sets;
using Sets.Infrastructure.Data.Idempotency;

namespace Sets.Infrastructure.Data;

/// <summary>
/// Контекст модуля Sets. Все таблицы живут в схеме sets и принадлежат владельцу модуля (ADR 0029).
/// </summary>
internal sealed class SetsDbContext(DbContextOptions<SetsDbContext> options) : DbContext(options)
{
    public DbSet<Set> Sets => Set<Set>();

    public DbSet<Element> Elements => Set<Element>();

    public DbSet<IdempotencyKeyRecord> IdempotencyKeys => Set<IdempotencyKeyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sets");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SetsDbContext).Assembly);
    }
}
