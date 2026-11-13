using System.Text.Json;
using Common.Application.DomainEvents;
using Common.Domain.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Доставка outbox-сообщений одной полосы. Сообщение обрабатывается в одной транзакции:
/// десериализация в доменное событие и его доставка обработчикам; исход фиксируется
/// как processed / retrying / dead. Полоса fast - in-process и транзакционна, аренда не нужна
/// (её используют будущие slow-обработчики с внешними эффектами).
/// </summary>
internal sealed class OutboxProcessor(
    SetsDbContext context,
    IDomainEventDispatcher dispatcher,
    TimeProvider clock,
    ILogger<OutboxProcessor> logger)
{
    private static readonly TimeSpan[] Backoff =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(25),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromHours(1),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Обрабатывает пачку готовых сообщений полосы и возвращает число доставленных.</summary>
    public async Task<int> ProcessBatchAsync(string lane, int batchSize, int maxAttempts, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var ids = await LockDueIdsAsync(lane, batchSize, cancellationToken);
        if (ids.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        var messages = await context.OutboxMessages
            .Where(message => ids.Contains(message.Id))
            .OrderBy(message => message.Id)
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow().UtcDateTime;
        var delivered = 0;

        foreach (var message in messages)
        {
            message.LastProcessedAt = now;

            var domainEvent = Deserialize(message);
            if (domainEvent is null)
            {
                Fail(message, now, maxAttempts, $"Неизвестный тип события '{message.Type}'.");
                continue;
            }

            var dispatch = await dispatcher.DispatchAsync([domainEvent], cancellationToken);
            if (dispatch.IsError)
            {
                Fail(message, now, maxAttempts, string.Join("; ", dispatch.Errors.Select(error => error.Code)));
                continue;
            }

            message.Status = OutboxStatus.Processed;
            message.FinishedAt = now;
            message.LastError = null;
            delivered++;
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return delivered;
    }

    /// <summary>
    /// Берёт идентификаторы готовых сообщений и удерживает строки до конца транзакции:
    /// второй инстанс приложения увидит соседние сообщения, а не те же самые.
    /// </summary>
    private async Task<List<long>> LockDueIdsAsync(string lane, int batchSize, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = context.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = """
            SELECT id
            FROM sets.outbox_messages
            WHERE lane = @lane AND status IN ('pending', 'retrying') AND next_attempt_at <= now()
            ORDER BY id
            FOR UPDATE SKIP LOCKED
            LIMIT @batch_size
            """;

        var laneParameter = command.CreateParameter();
        laneParameter.ParameterName = "lane";
        laneParameter.Value = lane;
        command.Parameters.Add(laneParameter);

        var batchParameter = command.CreateParameter();
        batchParameter.ParameterName = "batch_size";
        batchParameter.Value = batchSize;
        command.Parameters.Add(batchParameter);

        var ids = new List<long>(batchSize);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(reader.GetInt64(0));

        return ids;
    }

    private static IDomainEvent? Deserialize(OutboxMessage message)
    {
        var eventType = DomainEventTypeRegistry.Resolve(message.Type);
        if (eventType is null)
            return null;

        return JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions) as IDomainEvent;
    }

    /// <summary>Фиксирует неудачу: до исчерпания попыток - retrying с паузой, после - dead (только вручную).</summary>
    private void Fail(OutboxMessage message, DateTime now, int maxAttempts, string error)
    {
        message.Attempts++;
        message.LastError = error.Length > 1024 ? error[..1024] : error;

        if (message.Attempts >= maxAttempts)
        {
            message.Status = OutboxStatus.Dead;
            message.FinishedAt = now;
            logger.LogError(
                "Outbox message {MessageId} of type {Type} is dead after {Attempts} attempts: {Error}",
                message.Id, message.Type, message.Attempts, error);
            return;
        }

        message.Status = OutboxStatus.Retrying;
        message.NextAttemptAt = now + Backoff[Math.Min(message.Attempts - 1, Backoff.Length - 1)];
    }
}
