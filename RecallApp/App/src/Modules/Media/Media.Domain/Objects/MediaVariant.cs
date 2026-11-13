using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Производный вариант изображения: отдельный объект хранилища со своими размерами и весом.
/// Существует только в составе готового медиа-объекта (онтология Media §4.1).
/// </summary>
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

    public Guid MediaId { get; private set; }

    public MediaVariantName Variant { get; private set; }

    public ObjectKey ObjectKey { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public long SizeBytes { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>Создаёт вариант, проверяя, что длинная сторона укладывается в предел своего варианта.</summary>
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
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Неизвестный вариант изображения."),
        };

        if (Math.Max(width, height) > maxSide)
            return MediaErrors.ResolutionTooLarge;

        return new MediaVariant(mediaId, variant, ObjectKey.Variant(ownerId, mediaId, variant), width, height, sizeBytes, now);
    }
}
