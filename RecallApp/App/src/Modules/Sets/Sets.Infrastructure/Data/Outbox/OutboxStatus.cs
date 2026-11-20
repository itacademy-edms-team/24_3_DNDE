namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Содержит состояния жизненного цикла outbox-сообщения.
/// </summary>
/// <remarks>
/// Хранятся строками в snake_case - так же, как состояния аренды идемпотентности (ADR 0040).
/// Константы вместо enum: значение попадает в SQL-фильтры без конвертера.
/// </remarks>
internal static class OutboxStatus
{
    /// <summary>
    /// Ожидает доставки.
    /// </summary>
    public const string Pending = "pending";

    /// <summary>
    /// Взято в работу и обрабатывается; привязано к аренде.
    /// </summary>
    public const string Processing = "processing";

    /// <summary>
    /// Доставлено; повторно не берётся.
    /// </summary>
    public const string Processed = "processed";

    /// <summary>
    /// Обработка не удалась, сообщение ждёт повтора.
    /// </summary>
    public const string Retrying = "retrying";

    /// <summary>
    /// Исчерпаны попытки: доставка прекращена, нужна ручная реакция.
    /// </summary>
    public const string Dead = "dead";
}
