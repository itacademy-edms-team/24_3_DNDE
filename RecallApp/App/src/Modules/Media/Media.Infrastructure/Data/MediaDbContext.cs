using Media.Domain.Objects;
using Media.Domain.References;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data;

/// <summary>
/// Контекст модуля Media. Все таблицы живут в схеме media и принадлежат владельцу модуля (ADR 0029).
/// </summary>
internal sealed class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<MediaObject> MediaObjects => Set<MediaObject>();

    public DbSet<MediaVariant> MediaVariants => Set<MediaVariant>();

    public DbSet<MediaReference> MediaReferences => Set<MediaReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("media");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
