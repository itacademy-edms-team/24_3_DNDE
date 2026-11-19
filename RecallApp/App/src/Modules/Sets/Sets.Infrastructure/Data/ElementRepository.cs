using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Sets.Application.Abstractions;
using Sets.Application.Elements;
using Sets.Domain.Elements;

namespace Sets.Infrastructure.Data;

/// <summary>
/// Реализация хранилища элементов поверх EF Core.
/// </summary>
/// <remarks>
/// <para>Позиция хранится строкой: и сортировка, и keyset-курсор идут по ней, а не по числу.</para>
/// <para>Колонка объявлена с <c>COLLATE "C"</c>, поэтому база сравнивает строки побайтово -
/// порядок совпадает с тем, что вычисляет генератор рангов (ADR 0041).</para>
/// <para>Операции над составом выполняются под блокировкой строки набора (<see cref="ExecuteLockedAsync"/>):
/// ошибка внутри действия откатывает транзакцию.</para>
/// <para>Массовая пометка удаления (<see cref="MarkAllDeletedAsync"/>) выполняется одним UPDATE мимо домена:
/// набор к этому моменту уже проверен и загружен.</para>
/// </remarks>
internal sealed class ElementRepository(SetsDbContext context) : IElementRepository
{
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
            // Ранг хранится строкой: keyset идёт по (position, id) в лексикографическом порядке
            // (коллация "C" на колонке). string.Compare Npgsql транслирует в сравнение строк.
            var cursorPosition = after.Position;
            var cursorId = after.Id;

            query = query.Where(element =>
                string.Compare(element.Position, cursorPosition) > 0
                || (string.Compare(element.Position, cursorPosition) == 0 && element.Id > cursorId));
        }

        return await query
            .OrderBy(element => element.Position)
            .ThenBy(element => element.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Element>> ListForOrderingAsync(
        Guid setId,
        CancellationToken cancellationToken) =>
        await context.Elements
            .AsNoTracking()
            .Where(element => element.SetId == setId && element.DeletedAt == null)
            .OrderBy(element => element.Position)
            .ToListAsync(cancellationToken);

    public Task<Element?> GetLastAsync(Guid setId, CancellationToken cancellationToken) =>
        context.Elements
            .AsNoTracking()
            .Where(element => element.SetId == setId && element.DeletedAt == null)
            .OrderByDescending(element => element.Position)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Element?> FindAsync(Guid setId, Guid elementId, CancellationToken cancellationToken) =>
        context.Elements.SingleOrDefaultAsync(
            element => element.Id == elementId && element.SetId == setId && element.DeletedAt == null,
            cancellationToken);

    public Task<int> CountActiveAsync(Guid setId, CancellationToken cancellationToken) =>
        context.Elements.CountAsync(
            element => element.SetId == setId && element.DeletedAt == null,
            cancellationToken);

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

        // Ошибка внутри лока - откат: иначе частичные записи (например, зарезервированный ключ
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

    public Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken) =>
        context.Elements
            .Where(element => element.SetId == setId && element.DeletedAt == null)
            .ExecuteUpdateAsync(update => update
                .SetProperty(element => element.DeletedAt, utcNow)
                .SetProperty(element => element.UpdatedAt, utcNow), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Блокирует строку набора до конца текущей транзакции.
    /// </summary>
    /// <remarks>
    /// Запрос выполняется ради блокировки: результат не используется, поэтому выборка идёт без отслеживания.
    /// Без <c>FOR UPDATE</c> параллельная вставка могла бы проскочить между подсчётом элементов и их записью.
    /// </remarks>
    private Task LockSetAsync(Guid setId, CancellationToken cancellationToken) =>
        context.Sets
            .FromSqlInterpolated(
                $"SELECT id, owner_id, title, description, created_at, updated_at, deleted_at FROM sets.sets WHERE id = {setId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
