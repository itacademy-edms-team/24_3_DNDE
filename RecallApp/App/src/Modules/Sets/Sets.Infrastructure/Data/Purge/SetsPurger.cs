using Microsoft.EntityFrameworkCore;

namespace Sets.Infrastructure.Data.Purge;

/// <summary>
/// Физическое удаление мягко удалённых наборов и элементов.
/// </summary>
internal static class SetsPurger
{
    /// <summary>
    /// Удаляет строки с <c>deleted_at &lt;= cutoff</c> порциями.
    /// Сначала наборы: каскад FK забирает их элементы.
    /// Оставшиеся мягко удалённые элементы удаляются вторым проходом.
    /// </summary>
    /// <returns>Сколько наборов и элементов удалено физически.</returns>
    public static async Task<(int Sets, int Elements)> PurgeAsync(
        SetsDbContext context,
        DateTime cutoff,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var sets = await DeleteInBatchesAsync(context, cutoff, batchSize, cancellationToken);
        var elements = await DeleteElementBatchesAsync(context, cutoff, batchSize, cancellationToken);
        return (sets, elements);
    }

    private static async Task<int> DeleteInBatchesAsync(
        SetsDbContext context,
        DateTime cutoff,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var total = 0;

        while (true)
        {
            var deleted = await context.Sets
                .Where(set => set.DeletedAt != null && set.DeletedAt <= cutoff)
                .OrderBy(set => set.DeletedAt)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);

            total += deleted;
            if (deleted < batchSize)
                return total;
        }
    }

    private static async Task<int> DeleteElementBatchesAsync(
        SetsDbContext context,
        DateTime cutoff,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var total = 0;

        while (true)
        {
            var deleted = await context.Elements
                .Where(element => element.DeletedAt != null && element.DeletedAt <= cutoff)
                .OrderBy(element => element.DeletedAt)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);

            total += deleted;
            if (deleted < batchSize)
                return total;
        }
    }
}
