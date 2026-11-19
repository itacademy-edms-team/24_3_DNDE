namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Содержит настройки фоновой чистки журнала идемпотентности (секция <c>Sets:Idempotency</c>).
/// </summary>
internal sealed class IdempotencyCleanupOptions
{
    /// <summary>Как часто запускать чистку.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Сколько строк удалять за один заход.</summary>
    public int BatchSize { get; set; } = 500;
}
