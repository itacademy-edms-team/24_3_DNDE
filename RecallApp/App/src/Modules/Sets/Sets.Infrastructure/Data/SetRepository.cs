using Microsoft.EntityFrameworkCore;
using Sets.Application.Abstractions;
using Sets.Application.Sets;
using Sets.Domain.Sets;

namespace Sets.Infrastructure.Data;

internal sealed class SetRepository(SetsDbContext db) : ISetRepository
{
    public async Task<IReadOnlyList<Set>> ListOwnedAsync(
        Guid ownerId,
        SetCursor? after,
        int take,
        CancellationToken cancellationToken)
    {
        var query = db.Sets
            .AsNoTracking()
            .Where(set => set.OwnerId == ownerId && set.DeletedAt == null);

        if (after is not null)
        {
            // Курсорная пагинация (ADR 0039): сравнение кортежа ключей, а не смещение.
            // EF.Functions.LessThan транслируется Npgsql-провайдером в row comparison
            // (created_at, id) < (@cursor_created_at, @cursor_id) - по нему работает индекс
            // (owner_id, created_at DESC, id DESC). Guid как обычное поле сравнивать нельзя
            // (нет операторов <), поэтому сравнивается кортеж целиком.
            var cursorCreatedAt = after.CreatedAt.Kind == DateTimeKind.Utc
                ? after.CreatedAt
                : after.CreatedAt.ToUniversalTime();
            var cursorId = after.Id;

            query = query.Where(set => EF.Functions.LessThan(
                ValueTuple.Create(set.CreatedAt, set.Id),
                ValueTuple.Create(cursorCreatedAt, cursorId)));
        }

        return await query
            .OrderByDescending(set => set.CreatedAt)
            .ThenByDescending(set => set.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<Set?> FindOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken) =>
        db.Sets.SingleOrDefaultAsync(
            set => set.Id == id && set.OwnerId == ownerId && set.DeletedAt == null,
            cancellationToken);

    public void Add(Set set) => db.Sets.Add(set);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
