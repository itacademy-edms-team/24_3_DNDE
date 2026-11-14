namespace Media.Domain.Objects;

/// <summary>
/// Числовые границы среза (ADR 0037).
/// </summary>
public static class MediaConstraints
{
    /// <summary>Максимальный размер загружаемого файла: 10 МБ. Режется политикой хранилища и проверяется на confirm.</summary>
    public const long MaxUploadBytes = 10L * 1024 * 1024;

    /// <summary>Минимальный размер загружаемого файла: пустой файл изображением не является.</summary>
    public const long MinUploadBytes = 1;

    /// <summary>Максимум пикселей до декодирования - защита от декомпрессионных бомб.</summary>
    public const long MaxPixels = 25_000_000;

    /// <summary>Максимум по длинной стороне оригинала.</summary>
    public const int MaxSide = 8000;

    /// <summary>Длинная сторона варианта `thumb`.</summary>
    public const int ThumbMaxSide = 400;

    /// <summary>Длинная сторона варианта `display`.</summary>
    public const int DisplayMaxSide = 1600;

    /// <summary>Сколько вариантов обязано быть у готового объекта.</summary>
    public const int VariantsPerObject = 2;

    /// <summary>Срок жизни политики загрузки.</summary>
    public static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(10);

    /// <summary>Срок жизни ссылки на чтение.</summary>
    public static readonly TimeSpan ReadLinkLifetime = TimeSpan.FromMinutes(15);

    /// <summary>Через сколько брошенная загрузка считается осиротевшей.</summary>
    public static readonly TimeSpan PendingRetention = TimeSpan.FromHours(24);

    /// <summary>Через сколько готовый объект без ссылок считается осиротевшей.</summary>
    public static readonly TimeSpan UnreferencedRetention = TimeSpan.FromHours(24);

    /// <summary>Сколько незавершённых загрузок может быть у одного пользователя одновременно.</summary>
    public const int MaxPendingPerOwner = 5;

    /// <summary>Сколько идентификаторов можно запросить в одном запросе на подпись ссылок.</summary>
    public const int MaxLinksPerRequest = 100;
}
