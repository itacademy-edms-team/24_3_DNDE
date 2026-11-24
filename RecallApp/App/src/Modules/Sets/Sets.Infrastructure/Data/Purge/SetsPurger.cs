using Microsoft.EntityFrameworkCore;

namespace Sets.Infrastructure.Data.Purge;

/// <summary>
/// Предоставляет физическое удаление мягко удалённых наборов и элементов.
/// </summary>
internal static class SetsPurger
{
    /// <summary>
    /// Порционно удаляет строки, отмеченные не позднее <paramref name="cutoff"/>.
    /// </summary>
    /// <remarks>
    /// <para>Сначала удаляются наборы (элементы удаляются каскадом).</para>
    /// <para>Второй проход удаляет элементы, чьи наборы остались живы.</para>
    /// <para>Каждый проход идёт порциями, пока очередная порция не окажется неполной.</para>
    /// </remarks>
    /// <param name="context">Контекст модуля Sets.</param>
    /// <param name="cutoff">Граница: удаляются строки, помеченные не позже этого момента.</param>
    /// <param name="batchSize">Сколько строк удалять за один заход.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
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
