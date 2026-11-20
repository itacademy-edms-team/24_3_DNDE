namespace Media.Domain.Objects;

/// <summary>
/// Предоставляет ограничения домена Media.
/// </summary>
/// <remarks>
/// <para>По этим значениям проверяется загрузка, обработка и выдача ссылок (ADR 0037).</para>
/// <para>Значения зафиксированы в онтологии Media: менять их можно только вместе с документом,
/// иначе разойдутся код и описание.
/// Тест <c>MediaConstraints_Values_MatchOntology</c> сверяет одно с другим.</para>
/// </remarks>
public static class MediaConstraints
{
    /// <summary>
    /// Максимальный размер загружаемого файла: 10 МБ.
    /// </summary>
    public const long MaxUploadBytes = 10L * 1024 * 1024;

    /// <summary>
    /// Минимальный размер загружаемого файла: пустой файл изображением не является.
    /// </summary>
    public const long MinUploadBytes = 1;

    /// <summary>
    /// Максимум пикселей до декодирования - защита от декомпрессионных бомб.
    /// </summary>
    public const long MaxPixels = 25_000_000;

    /// <summary>
    /// Максимум по длинной стороне оригинала в пикселях.
    /// </summary>
    public const int MaxSide = 8000;

    /// <summary>
    /// Максимум длинной стороны варианта <c>thumb</c> в пикселях.
    /// </summary>
    public const int ThumbMaxSide = 400;

    /// <summary>
    /// Максимум длинной стороны варианта <c>display</c> в пикселях.
    /// </summary>
    public const int DisplayMaxSide = 1600;

    /// <summary>
    /// Сколько вариантов обязано быть у готового объекта.
    /// </summary>
    public const int VariantsPerObject = 2;

    /// <summary>
    /// Срок жизни политики загрузки: сколько даётся на отправку файла.
    /// </summary>
    public static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Срок жизни ссылки на чтение.
    /// </summary>
    public static readonly TimeSpan ReadLinkLifetime = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Сколько незавершённых загрузок может быть у одного пользователя одновременно.
    /// </summary>
    public const int MaxPendingPerOwner = 5;

    /// <summary>
    /// Сколько идентификаторов можно запросить в одном запросе на подпись ссылок.
    /// </summary>
    public const int MaxLinksPerRequest = 100;
}
