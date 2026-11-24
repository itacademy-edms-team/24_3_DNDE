namespace Sets.Infrastructure.Data.Purge;

/// <summary>
/// Содержит настройки фоновой физической чистки мягко удалённого контента (секция <c>Sets:Purge</c>).
/// </summary>
internal sealed class SetsPurgeOptions
{
    /// <summary>
    /// Возвращает или задает срок хранения удалённого набора и его элементов до физического
    /// удаления.
    /// </summary>
    /// <remarks>
    /// ADR 0036: окно retention.
    /// </remarks>
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(30);

    /// <summary>Возвращает или задает интервал запуска чистки.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Возвращает или задает максимум строк, удаляемых за заход.</summary>
    public int BatchSize { get; set; } = 500;
}
