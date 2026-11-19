namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Представляет outbox-сообщение: доменное событие,
/// материализованное в строку таблицы и ожидающее доставки.
/// </summary>
/// <remarks>
/// Запись создаётся в той же транзакции, что и доменное изменение, поэтому потерять событие
/// при успешном коммите нельзя. Жизненным циклом управляет воркер (ADR 0040).
/// </remarks>
internal sealed class OutboxMessage
{
    /// <summary>
    /// Возвращает или задает идентификатор сообщения; монотонный, задаёт порядок выдачи.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Возвращает или задает стабильное имя события - ключ маршрутизации и десериализации.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Возвращает или задает версию схемы payload; текущая версия - 1.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Возвращает или задает полосу доставки: <c>fast</c> или <c>slow</c>.
    /// </summary>
    public string Lane { get; set; } = null!;

    /// <summary>
    /// Возвращает или задает сериализованное доменное событие (jsonb).
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Возвращает или задает статус жизненного цикла; значения - в <see cref="OutboxStatus"/>.
    /// </summary>
    public string Status { get; set; } = OutboxStatus.Pending;

    /// <summary>
    /// Возвращает или задает чекпоинт обработчика: непрозрачный jsonb, воркер его не интерпретирует.
    /// </summary>
    public string? Checkpoint { get; set; }

    /// <summary>
    /// Возвращает или задает число предпринятых попыток доставки.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Возвращает или задает момент в будущем в UTC, начиная с которого сообщение снова можно
    /// брать в обработку.
    /// </summary>
    public DateTime NextAttemptAt { get; set; }

    /// <summary>
    /// Возвращает или задает владельца аренды; используется slow-полосой, fast-полоса держит
    /// строки транзакцией.
    /// </summary>
    public Guid? LeaseOwner { get; set; }

    /// <summary>
    /// Возвращает или задает срок аренды (момент в будущем в UTC); истёкшую перехватывает
    /// другой воркер (slow-полоса).
    /// </summary>
    public DateTime? LeaseUntil { get; set; }

    /// <summary>Возвращает или задает последнюю ошибку доставки; обрезается до 1024 символов.</summary>
    public string? LastError { get; set; }

    /// <summary>Возвращает или задает момент создания в UTC - в той же транзакции, что и доменное
    /// изменение.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Возвращает или задает момент в UTC последней попытки обработки сообщения.</summary>
    public DateTime? LastProcessedAt { get; set; }

    /// <summary>Возвращает или задает момент завершения в UTC: сообщение доставлено или признано
    /// dead.</summary>
    public DateTime? FinishedAt { get; set; }
}
