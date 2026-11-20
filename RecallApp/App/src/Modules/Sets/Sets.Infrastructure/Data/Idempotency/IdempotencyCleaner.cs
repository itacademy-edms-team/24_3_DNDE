using Microsoft.EntityFrameworkCore;

namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Удаляет истёкшие ключи идемпотентности.
/// </summary>
internal static class IdempotencyCleaner
{
    /// <summary>
    /// Удаляет ключи, срок которых истёк, порциями.
    /// </summary>
    /// <remarks>
    /// Удаление идёт порциями, пока очередная не окажется неполной.
    /// </remarks>
    /// <param name="context">Контекст модуля Sets.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <param name="batchSize">Сколько строк удалять за один заход.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сколько строк удалено.</returns>
    public static async Task<int> DeleteExpiredAsync(
        SetsDbContext context,
        DateTime now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var total = 0;

        while (true)
        {
            var deleted = await context.IdempotencyKeys
                .Where(record => record.ExpiresAt <= now)
                .OrderBy(record => record.ExpiresAt)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);

            total += deleted;
            if (deleted < batchSize)
                return total;
        }
    }
}
