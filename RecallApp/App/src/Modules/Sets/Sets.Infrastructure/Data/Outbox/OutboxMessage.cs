namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Outbox-сообщение: доменное событие, материализованное в строку таблицы и ожидающее доставки.
/// Жизненный цикл ведёт воркер; payload хранит сериализованное событие (ADR 0040).
/// </summary>
internal sealed class OutboxMessage
{
    public long Id { get; set; }

    /// <summary>Стабильное имя события - ключ маршрутизации и десериализации.</summary>
    public string Type { get; set; } = null!;

    /// <summary>Версия схемы payload; существующие строки по умолчанию v1.</summary>
    public int Version { get; set; }

    /// <summary>Полоса доставки: fast | slow.</summary>
    public string Lane { get; set; } = null!;

    /// <summary>Сериализованное доменное событие (jsonb).</summary>
    public string Payload { get; set; } = null!;

    public string Status { get; set; } = OutboxStatus.Pending;

    /// <summary>Чекпоинт обработчика: непрозрачное jsonb, воркер его не интерпретирует.</summary>
    public string? Checkpoint { get; set; }

    public int Attempts { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public Guid? LeaseOwner { get; set; }

    public DateTime? LeaseUntil { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastProcessedAt { get; set; }

    public DateTime? FinishedAt { get; set; }
}
