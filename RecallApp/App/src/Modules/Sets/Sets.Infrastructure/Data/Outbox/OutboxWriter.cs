using System.Text.Json;
using Sets.Application.Abstractions;
using Sets.Infrastructure.Data;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Писатель outbox-сообщений в схему sets.
/// Сериализация фиксирована: camelCase, как у контрактов модулей,
/// чтобы потребитель мог разобрать payload тем же набором настроек.
/// </summary>
internal sealed class OutboxWriter(SetsDbContext context, TimeProvider clock) : IOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue(string type, object payload)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        context.OutboxMessages.Add(new OutboxMessage
        {
            Type = type,
            Payload = JsonSerializer.Serialize(payload, JsonOptions),
            CreatedAt = now,
            NextAttemptAt = now,
            Attempts = 0,
        });
    }
}
