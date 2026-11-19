namespace Media.Infrastructure.Data.Cleanup;

/// <summary>
/// Содержит настройки фоновой чистки осиротевших данных медиа (секция <c>Media:Cleanup</c>).
/// </summary>
internal sealed class MediaCleanupOptions
{
    /// <summary>Возвращает или задает срок, через который загрузка в <c>pending</c> или
    /// <c>failed</c> удаляется вместе с объектом в <c>tmp/</c>.</summary>
    public TimeSpan AbandonedRetention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Возвращает или задает срок, через который готовый файл без ссылок считается
    /// сиротой (онтология Media §6.4).</summary>
    public TimeSpan UnreferencedRetention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Возвращает или задает срок, через который надгробие <c>deleted</c> удаляется из БД;
    /// варианты удаляются из хранилища перед строкой.</summary>
    public TimeSpan DeletedRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Возвращает или задает интервал запуска чистки.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Возвращает или задает значение, указывающее, чистить ли все брошенные загрузки перед
    /// первым циклом, без оглядки на ретенцию.
    /// </summary>
    /// <remarks>
    /// Для локальной разработки: чтобы рестарт возвращал чистое состояние по загрузкам.
    /// В производственном окружении не включать.
    /// </remarks>
    public bool PurgeOnStartup { get; set; }

    /// <summary>Возвращает или задает число записей, обрабатываемых за один заход.</summary>
    public int BatchSize { get; set; } = 500;
}
