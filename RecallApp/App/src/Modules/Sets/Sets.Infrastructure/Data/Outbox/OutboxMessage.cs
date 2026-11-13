namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Сообщение outbox: факт, который нужно доставить другому модулю, записанный в той же транзакции,
/// что и бизнес-изменение (ADR 0040). Состояние строки показывает, доставлено ли сообщение.
/// </summary>
internal sealed class OutboxMessage
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    /// <summary>Тело сообщения в jsonb: форму знает обработчик, продюсер её не интерпретирует.</summary>
    public string Payload { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    /// <summary>Когда сообщение можно взять в работу: сдвигается назад при повторах.</summary>
    public DateTime NextAttemptAt { get; set; }

    public int Attempts { get; set; }

    public DateTime? ProcessedAt { get; set; }

    /// <summary>Заполняется, когда попытки исчерпаны: сообщение требует ручного разбора.</summary>
    public DateTime? AbandonedAt { get; set; }

    public string? LastError { get; set; }
}
