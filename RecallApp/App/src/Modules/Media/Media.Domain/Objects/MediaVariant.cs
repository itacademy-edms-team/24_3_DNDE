using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Представляет производный вариант изображения - отдельный объект хранилища
/// со своими размерами и весом.
/// </summary>
/// <remarks>
/// <para>Существует только в составе готового медиа-объекта: набор вариантов фиксируется
/// одновременно с метаданными исходника, отдельно вариант не создаётся (онтология Media §4.1).</para>
/// <para>Ключ в хранилище вычисляется из владельца, медиа-объекта и вида варианта, поэтому
/// повторная обработка попадает в тот же ключ.</para>
/// </remarks>
public sealed class MediaVariant
{
    private MediaVariant() { }

    private MediaVariant(
        Guid mediaId,
        MediaVariantName variant,
        ObjectKey objectKey,
        int width,
        int height,
        long sizeBytes,
        DateTime createdAt)
    {
        MediaId = mediaId;
        Variant = variant;
        ObjectKey = objectKey;
        Width = width;
        Height = height;
        SizeBytes = sizeBytes;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Возвращает идентификатор медиа-объекта, которому принадлежит вариант.
    /// </summary>
    public Guid MediaId { get; private set; }

    /// <summary>
    /// Возвращает вид варианта.
    /// </summary>
    public MediaVariantName Variant { get; private set; }

    /// <summary>
    /// Возвращает ключ объекта в хранилище.
    /// </summary>
    public ObjectKey ObjectKey { get; private set; }

    /// <summary>
    /// Возвращает ширину варианта в пикселях.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Возвращает высоту варианта в пикселях.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Возвращает размер файла варианта в байтах.
    /// </summary>
    public long SizeBytes { get; private set; }

    /// <summary>
    /// Возвращает момент создания варианта в UTC.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Создаёт вариант, проверяя, что длинная сторона укладывается в предел своего вида.
    /// </summary>
    /// <remarks>
    /// <para>Хранилище не затрагивается: метод только описывает будущий объект, запись байтов
    /// выполняет обработчик изображений.</para>
    /// <para>Ключ объекта образуется из владельца, медиа-объекта и вида варианта, поэтому он
    /// детерминирован. Это позволяет при повторной записи того же файла перезаписать объект,
    /// а не завести дубликат в объектном хранилище.</para>
    /// </remarks>
    /// <param name="mediaId">Идентификатор медиа-объекта, к которому относится вариант.</param>
    /// <param name="ownerId">Владелец медиа-объекта.</param>
    /// <param name="variant">Вид варианта; задаёт предел длинной стороны.</param>
    /// <param name="width">Ширина в пикселях.</param>
    /// <param name="height">Высота в пикселях.</param>
    /// <param name="sizeBytes">Размер файла в байтах.</param>
    /// <param name="now">Текущий момент в UTC; задаётся вызывающим</param>
    /// <returns>
    /// Новый вариант либо ошибка:
    /// <see cref="MediaErrors.ImageCorrupt"/> (неположительные размеры)
    /// или <see cref="MediaErrors.ResolutionTooLarge"/> (длинная сторона больше предела вида).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Значение <paramref name="variant"/> не определено в <see cref="MediaVariantName"/>.
    /// </exception>
    public static ErrorOr<MediaVariant> Create(
        Guid mediaId,
        Guid ownerId,
        MediaVariantName variant,
        int width,
        int height,
        long sizeBytes,
        DateTime now)
    {
        if (width <= 0 || height <= 0)
            return MediaErrors.ImageCorrupt;

        if (sizeBytes <= 0)
            return MediaErrors.ImageCorrupt;

        var maxSide = variant switch
        {
            MediaVariantName.Thumb => MediaConstraints.ThumbMaxSide,
            MediaVariantName.Display => MediaConstraints.DisplayMaxSide,
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown image variant."),
        };

        if (Math.Max(width, height) > maxSide)
            return MediaErrors.ResolutionTooLarge;

        return new MediaVariant(mediaId, variant, ObjectKey.Variant(ownerId, mediaId, variant), width, height, sizeBytes, now);
    }
}
