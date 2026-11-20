namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Содержит настройки фоновой чистки хранилища идемпотентности (секция <c>Sets:Idempotency</c>).
/// </summary>
internal sealed class IdempotencyCleanupOptions
{
    /// <summary>Возвращает или задает интервал запуска чистки.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Возвращает или задает число строк, удаляемых за один заход.</summary>
    public int BatchSize { get; set; } = 500;
}
