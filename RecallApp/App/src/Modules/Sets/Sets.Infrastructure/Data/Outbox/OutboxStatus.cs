namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Состояния жизненного цикла outbox-сообщения (ADR 0040, §5). Хранятся строками в snake_case,
/// как и состояния lease-идемпотентности.
/// </summary>
internal static class OutboxStatus
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Processed = "processed";
    public const string Retrying = "retrying";
    public const string Dead = "dead";
}
