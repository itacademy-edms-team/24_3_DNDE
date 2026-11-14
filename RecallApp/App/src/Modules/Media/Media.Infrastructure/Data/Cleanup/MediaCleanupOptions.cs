namespace Media.Infrastructure.Data.Cleanup;

/// <summary>
/// Настройки фоновой чистки сирот медиа (секция <c>Media:Cleanup</c>).
/// </summary>
internal sealed class MediaCleanupOptions
{
    /// <summary>Через сколько брошенная загрузка (`pending`/`failed`) удаляется вместе с объектом в `tmp/`.</summary>
    public TimeSpan AbandonedRetention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Через сколько готовый файл без ссылок считается сиротой (онтология Media §6.4).</summary>
    public TimeSpan UnreferencedRetention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Через сколько надгробие `deleted` удаляется из БД: объекты к этому моменту уже убраны.</summary>
    public TimeSpan DeletedRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Интервал запуска чистки.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Чистить ли все брошенные загрузки перед первым циклом, без оглядки на ретенцию.
    /// Для локальной разработки: чтобы рестарт возвращал чистое состояние по загрузкам.
    /// В производственном окружении не включать.
    /// </summary>
    public bool PurgeOnStartup { get; set; }

    /// <summary>Сколько записей обрабатывать за один заход.</summary>
    public int BatchSize { get; set; } = 500;
}
