using System.Text.Json;
using Common.Application.Outbox;
using Common.Domain.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Преобразует доменные события сущностей в outbox-записи в той же транзакции.
/// </summary>
/// <remarks>
/// <para>На <c>SavingChanges</c> перехватчик сканирует отслеживаемые сущности
/// (<see cref="IHasDomainEvents"/>), забирает накопленные события, очищает их у сущности
/// и добавляет строки outbox в тот же <c>SaveChanges</c>.</para>
/// <para>Поэтому доменное изменение и его событие коммитятся вместе: либо оба, либо ничего.
/// Это заменяет ручной <c>Enqueue</c> (ADR 0040).</para>
/// </remarks>
internal sealed class DomainEventsToOutboxInterceptor(TimeProvider clock) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var messages = Extract(context, clock.GetUtcNow().UtcDateTime);
        if (messages.Count > 0)
            context.Set<OutboxMessage>().AddRange(messages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static List<OutboxMessage> Extract(DbContext context, DateTime now)
    {
        var messages = new List<OutboxMessage>();

        foreach (var entry in context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            var events = entry.Entity.DomainEvents;
            if (events.Count == 0)
                continue;

            foreach (var domainEvent in events)
            {
                messages.Add(new OutboxMessage
                {
                    Type = domainEvent.EventType,
                    Version = 1,
                    // Пока единственное событие — fast; полоса на событие появится вместе с первым slow.
                    Lane = OutboxLanes.Fast,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions),
                    Status = OutboxStatus.Pending,
                    Attempts = 0,
                    NextAttemptAt = now,
                    CreatedAt = now,
                });
            }

            entry.Entity.ClearDomainEvents();
        }

        return messages;
    }
}
