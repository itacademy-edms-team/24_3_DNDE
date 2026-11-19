namespace Sets.Infrastructure.Data.Purge;

/// <summary>
/// Содержит настройки фоновой физической чистки мягко удалённого контента (секция <c>Sets:Purge</c>).
/// </summary>
internal sealed class SetsPurgeOptions
{
    /// <summary>
    /// Сколько хранить удалённый набор и его элементы до физического удаления.
    /// </summary>
    /// <remarks>
    /// ADR 0036: окно retention.
    /// </remarks>
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(30);

    /// <summary>Интервал запуска чистки.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Сколько строк максимум удалять за заход.</summary>
    public int BatchSize { get; set; } = 500;
}
