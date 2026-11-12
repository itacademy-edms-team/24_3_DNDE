using Microsoft.EntityFrameworkCore;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Infrastructure.Data;

internal sealed class SetRepository(SetsDbContext db) : ISetRepository
{
    public async Task<IReadOnlyList<Set>> ListOwnedAsync(Guid ownerId, CancellationToken cancellationToken) =>
        await db.Sets
            .AsNoTracking()
            .Where(set => set.OwnerId == ownerId && set.DeletedAt == null)
            .OrderByDescending(set => set.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task<Set?> FindOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken) =>
        db.Sets.SingleOrDefaultAsync(
            set => set.Id == id && set.OwnerId == ownerId && set.DeletedAt == null,
            cancellationToken);

    public void Add(Set set) => db.Sets.Add(set);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
