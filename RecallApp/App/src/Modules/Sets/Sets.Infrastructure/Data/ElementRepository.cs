using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Sets.Application.Abstractions;
using Sets.Application.Elements;
using Sets.Domain.Elements;

namespace Sets.Infrastructure.Data;

internal sealed class ElementRepository(SetsDbContext context) : IElementRepository
{
    /// <summary>
    /// Временный сдвиг позиций при вставке и перестановке: частичный уникальный индекс
    /// не позволяет двигать строки «на месте».
    /// </summary>
    private const int ShiftOffset = 1_000_000;

    public async Task<IReadOnlyList<Element>> ListBySetAsync(
        Guid setId,
        ElementCursor? after,
        int take,
        CancellationToken cancellationToken)
    {
        var query = context.Elements
            .AsNoTracking()
            .Where(element => element.SetId == setId && element.DeletedAt == null);

        if (after is not null)
        {
            // Сравнение кортежа (position, id) > курсора - Npgsql транслирует в row comparison (ADR 0039).
            var cursorPosition = after.Position;
            var cursorId = after.Id;

            query = query.Where(element => EF.Functions.GreaterThan(
                ValueTuple.Create(element.Position, element.Id),
                ValueTuple.Create(cursorPosition, cursorId)));
        }

        return await query
            .OrderBy(element => element.Position)
            .ThenBy(element => element.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<Element?> FindAsync(Guid setId, Guid elementId, CancellationToken cancellationToken) =>
        context.Elements.SingleOrDefaultAsync(
            element => element.Id == elementId && element.SetId == setId && element.DeletedAt == null,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListActiveIdsAsync(Guid setId, CancellationToken cancellationToken) =>
        await context.Elements
            .AsNoTracking()
            .Where(element => element.SetId == setId && element.DeletedAt == null)
            .OrderBy(element => element.Position)
            .Select(element => element.Id)
            .ToListAsync(cancellationToken);

    public async Task<TResult> ExecuteLockedAsync<TResult>(
        Guid setId,
        Func<int, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await LockSetAsync(setId, cancellationToken);

        var activeCount = await context.Elements
            .CountAsync(element => element.SetId == setId && element.DeletedAt == null, cancellationToken);

        var result = await action(activeCount, cancellationToken);

        // Ошибка внутри лока — откат: иначе частичные записи (например, зарезервированный ключ
        // идемпотентности) уедут в БД вместе с пустым коммитом.
        if (result is IErrorOr { IsError: true })
        {
            await transaction.RollbackAsync(cancellationToken);
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public void Add(Element element) => context.Elements.Add(element);

    public async Task<int> ShiftPositionsFromAsync(
        Guid setId,
        int fromPosition,
        CancellationToken cancellationToken)
    {
        // Фаза 1: уводим затрагиваемые строки за пределы диапазона.
        // pos = pos + ShiftOffset
        var shifted = await context.Elements
            .Where(element => element.SetId == setId
                && element.DeletedAt == null
                && element.Position >= fromPosition)
            .ExecuteUpdateAsync(
                update => update.SetProperty(element => element.Position, element => element.Position + ShiftOffset),
                cancellationToken);

        // Фаза 2: возвращаем в диапазон со сдвигом +1.
        // pos = pos - ShiftOffset + 1
        await context.Elements
            .Where(element => element.SetId == setId
                && element.DeletedAt == null
                && element.Position >= fromPosition + ShiftOffset)
            .ExecuteUpdateAsync(
                update => update.SetProperty(element => element.Position, element => element.Position - ShiftOffset + 1),
                cancellationToken);

        return shifted;
    }

    public Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken) =>
        context.Elements
            .Where(element => element.SetId == setId && element.DeletedAt == null)
            .ExecuteUpdateAsync(update => update
                .SetProperty(element => element.DeletedAt, utcNow)
                .SetProperty(element => element.UpdatedAt, utcNow), cancellationToken);

    public async Task<bool> DeleteAndCompactAsync(
        Guid setId,
        Guid elementId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var element = await context.Elements.SingleOrDefaultAsync(
            candidate => candidate.Id == elementId && candidate.SetId == setId && candidate.DeletedAt == null,
            cancellationToken);

        if (element is null)
            return false;

        var removedPosition = element.Position;
        element.MarkDeleted(utcNow);

        // Фиксируем удаление до сдвига: иначе сосед упрётся в частичный уникальный индекс.
        await context.SaveChangesAsync(cancellationToken);

        await context.Elements
            .Where(candidate => candidate.SetId == setId
                && candidate.DeletedAt == null
                && candidate.Position > removedPosition)
            .ExecuteUpdateAsync(
                update => update.SetProperty(candidate => candidate.Position, candidate => candidate.Position - 1),
                cancellationToken);

        return true;
    }

    public async Task ApplyOrderAsync(
        Guid setId,
        IReadOnlyList<Guid> orderedElementIds,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        // Фаза 1: уводим активные позиции за пределы целевого диапазона, чтобы снять коллизии индекса.
        await context.Elements
            .Where(element => element.SetId == setId && element.DeletedAt == null)
            .ExecuteUpdateAsync(
                update => update.SetProperty(element => element.Position, element => element.Position + ShiftOffset),
                cancellationToken);

        // Фаза 2: присваиваем позиции 0..n-1 в присланном порядке.
        for (var position = 0; position < orderedElementIds.Count; position++)
        {
            var elementId = orderedElementIds[position];
            var newPosition = position;

            await context.Elements
                .Where(element => element.Id == elementId && element.SetId == setId && element.DeletedAt == null)
                .ExecuteUpdateAsync(
                    update => update
                        .SetProperty(element => element.Position, newPosition)
                        .SetProperty(element => element.UpdatedAt, utcNow),
                    cancellationToken);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Блокирует набор на уровне БД для обновления.
    /// </summary>
    private Task LockSetAsync(Guid setId, CancellationToken cancellationToken) =>
        context.Sets
            .FromSqlInterpolated(
                $"SELECT id, owner_id, title, description, created_at, updated_at, deleted_at FROM sets.sets WHERE id = {setId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
