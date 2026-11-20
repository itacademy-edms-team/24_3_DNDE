namespace Common.Application.Outbox;

/// <summary>
/// Предоставляет полосы доставки outbox.
/// </summary>
/// <remarks>
/// Полосы разделяют сообщения по скорости обработки,
/// чтобы медленное сообщение не блокировало всю очередь (ADR 0040).
/// </remarks>
public static class OutboxLanes
{
    /// <summary>Быстрая полоса: обычные сообщения обрабатываются сразу.</summary>
    public const string Fast = "fast";

    /// <summary>Медленная полоса (email, фоновая обработка).</summary>
    public const string Slow = "slow";
}
