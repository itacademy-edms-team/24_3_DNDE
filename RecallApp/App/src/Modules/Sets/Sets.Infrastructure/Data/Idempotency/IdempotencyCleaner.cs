using Microsoft.EntityFrameworkCore;

namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Чистильщик истёкших ключей идемпотентности.
/// </summary>
internal static class IdempotencyCleaner
{
    /// <summary>
    /// Удаляет ключи с <c>expires_at &lt;= now</c> порциями по <paramref name="batchSize"/>:
    /// одна большая транзакция блокировала бы таблицу дольше, чем нужно.
    /// </summary>
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
