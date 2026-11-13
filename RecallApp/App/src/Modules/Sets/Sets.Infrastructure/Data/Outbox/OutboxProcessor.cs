using System.Data.Common;
using System.Text.Json;
using Common.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Доставка сообщений outbox зарегистрированным обработчикам: 
/// берёт пачку в порядке следования идентификаторов, применяет по
/// и фиксирует результат - доставлено, повторить позже или отправить на разбор.
/// Доставка at-least-once, поэтому обработчик обязан быть идемпотентным (ADR 0040).
/// </summary>
internal sealed class OutboxProcessor(
    SetsDbContext context,
    IEnumerable<IOutboxMessageHandler> handlers,
    IOptions<OutboxOptions> options,
    TimeProvider clock,
    ILogger<OutboxProcessor> logger)
{
    /// <summary>Паузы между попытками: 5 с, 25 с, 2 мин, 10 мин, дальше раз в час.</summary>
    private static readonly TimeSpan[] Backoff =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(25),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromHours(1),
    ];

    /// <summary>Обрабатывает одну пачку сообщений и возвращает число доставленных.</summary>
    public async Task<int> ProcessBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var ids = await LockPendingIdsAsync(batchSize, cancellationToken);
        if (ids.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        var messages = await context.OutboxMessages
            .Where(message => ids.Contains(message.Id))
            .OrderBy(message => message.Id)
            .ToListAsync(cancellationToken);

        var byType = handlers
            .GroupBy(handler => handler.MessageType, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var delivered = 0;
        var now = clock.GetUtcNow().UtcDateTime;

        foreach (var message in messages)
        {
            if (!byType.TryGetValue(message.Type, out var handler))
            {
                ScheduleRetry(message, now, $"Нет обработчика для типа '{message.Type}'.");
                continue;
            }

            try
            {
                using var payload = JsonDocument.Parse(message.Payload);
                var result = await handler.HandleAsync(payload.RootElement, cancellationToken);

                if (result.IsError)
                {
                    var codes = string.Join("; ", result.Errors.Select(error => error.Code));
                    ScheduleRetry(message, now, codes);
                    continue;
                }

                message.ProcessedAt = now;
                message.LastError = null;
                delivered++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Outbox message {MessageId} of type {Type} failed", message.Id, message.Type);
                ScheduleRetry(message, now, exception.Message);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return delivered;
    }

    /// <summary>
    /// Берёт идентификаторы готовых к доставке сообщений и удерживает строки до конца транзакции:
    /// второй инстанс приложения увидит соседние сообщения, а не те же самые.
    /// </summary>
    private async Task<List<long>> LockPendingIdsAsync(int batchSize, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = context.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = """
            SELECT id
            FROM sets.outbox_messages
            WHERE processed_at IS NULL AND abandoned_at IS NULL AND next_attempt_at <= now()
            ORDER BY id
            FOR UPDATE SKIP LOCKED
            LIMIT @batch_size
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "batch_size";
        parameter.Value = batchSize;
        command.Parameters.Add(parameter);

        var ids = new List<long>(batchSize);
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(reader.GetInt64(0));

        return ids;
    }

    /// <summary>
    /// Переносит попытку в будущее с нарастающей паузой; после лимита помечает сообщение брошенным,
    /// чтобы оно не крутилось вечно и было видно в логах.
    /// </summary>
    private void ScheduleRetry(OutboxMessage message, DateTime now, string error)
    {
        message.Attempts++;
        message.LastError = error.Length > 1024 ? error[..1024] : error;

        if (message.Attempts >= options.Value.MaxAttempts)
        {
            message.AbandonedAt = now;
            logger.LogError(
                "Outbox message {MessageId} of type {Type} abandoned after {Attempts} attempts: {Error}",
                message.Id, message.Type, message.Attempts, error);
            return;
        }

        var delay = Backoff[Math.Min(message.Attempts - 1, Backoff.Length - 1)];
        message.NextAttemptAt = now + delay;
    }
}
