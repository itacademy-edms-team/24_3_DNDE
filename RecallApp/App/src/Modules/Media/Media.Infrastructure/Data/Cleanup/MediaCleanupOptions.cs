namespace Media.Infrastructure.Data.Cleanup;

/// <summary>
/// Содержит настройки фоновой чистки осиротевших данных медиа (секция <c>Media:Cleanup</c>).
/// </summary>
internal sealed class MediaCleanupOptions
{
    /// <summary>Через сколько загрузка в <c>pending</c> или <c>failed</c> удаляется вместе с объектом
    /// в <c>tmp/</c>.</summary>
    public TimeSpan AbandonedRetention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Через сколько готовый файл без ссылок считается сиротой (онтология Media §6.4).</summary>
    public TimeSpan UnreferencedRetention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Через сколько надгробие <c>deleted</c> удаляется из БД; варианты удаляются
    /// из хранилища перед строкой.</summary>
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
