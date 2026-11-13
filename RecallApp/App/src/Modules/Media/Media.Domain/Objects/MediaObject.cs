using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Медиа-объект - корень агрегата: владелец, состояние жизненного цикла, метаданные исходника
/// и два готовых варианта. Байты живут в объектном хранилище, здесь только метаданные (ADR 0029/0037).
/// </summary>
public sealed class MediaObject
{
    private readonly List<MediaVariant> _variants = [];

    private MediaObject() { }

    private MediaObject(Guid id, Guid ownerId, string declaredContentType, DateTime createdAt, DateTime uploadExpiresAt)
    {
        Id = id;
        OwnerId = ownerId;
        DeclaredContentType = declaredContentType;
        Status = MediaStatus.Pending;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        UploadExpiresAt = uploadExpiresAt;
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public MediaStatus Status { get; private set; }

    /// <summary>Тип, который заявил клиент: он же вшит в политику загрузки.</summary>
    public string DeclaredContentType { get; private set; } = null!;

    public MediaFormat? SourceFormat { get; private set; }

    public long? SourceSizeBytes { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureDescription { get; private set; }

    /// <summary>Конец окна доверия к загрузке: позже подтверждать уже нечего.</summary>
    public DateTime UploadExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? ReadyAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyCollection<MediaVariant> Variants => _variants;

    /// <summary>Создаёт запись в состоянии `pending` и открывает окно загрузки.</summary>
    public static ErrorOr<MediaObject> Create(Guid ownerId, string? declaredContentType, DateTime now)
    {
        if (ownerId == Guid.Empty)
            return MediaErrors.NotFound;

        if (string.IsNullOrWhiteSpace(declaredContentType))
            return MediaErrors.ContentTypeUnsupported;

        return new MediaObject(
            Guid.NewGuid(),
            ownerId,
            declaredContentType.Trim().ToLowerInvariant(),
            now,
            now + MediaConstraints.UploadWindow);
    }

    /// <summary>Проверяет, что файл ещё можно подтверждать: не истекло окно и запись не закрыта.</summary>
    public ErrorOr<Success> EnsureConfirmable(DateTime now)
    {
        if (Status == MediaStatus.Deleted)
            return MediaErrors.NotFound;

        if (Status != MediaStatus.Pending)
            return MediaErrors.ObjectMissing;

        if (now > UploadExpiresAt)
            return MediaErrors.UploadExpired;

        return Result.Success;
    }

    /// <summary>
    /// Переводит объект в `ready`: фиксирует метаданные исходника и оба варианта.
    /// Проверки сконцентрированы здесь, чтобы никакой вызов не мог записать «полуготовый» файл.
    /// </summary>
    public ErrorOr<Success> MarkReady(
        MediaFormat format,
        int width,
        int height,
        long sourceSizeBytes,
        IReadOnlyCollection<MediaVariant> variants,
        DateTime now)
    {
        if (Status != MediaStatus.Pending)
            return MediaErrors.ObjectMissing;

        if (width <= 0 || height <= 0)
            return MediaErrors.ImageCorrupt;

        if (sourceSizeBytes is <= 0 or > MediaConstraints.MaxUploadBytes)
            return MediaErrors.FileTooLarge;

        if (Math.Max(width, height) > MediaConstraints.MaxSide || (long)width * height > MediaConstraints.MaxPixels)
            return MediaErrors.ResolutionTooLarge;

        if (!HasExactVariantSet(variants))
            return MediaErrors.ImageCorrupt;

        _variants.Clear();
        _variants.AddRange(variants);

        SourceFormat = format;
        SourceSizeBytes = sourceSizeBytes;
        Width = width;
        Height = height;
        Status = MediaStatus.Ready;
        ReadyAt = now;
        UpdatedAt = now;
        FailureCode = null;
        FailureDescription = null;
        return Result.Success;
    }

    /// <summary>Переводит объект в `failed`: окончательная непригодность файла, повторов не будет.</summary>
    public ErrorOr<Success> MarkFailed(Error error, DateTime now)
    {
        if (Status == MediaStatus.Deleted)
            return MediaErrors.NotFound;

        if (Status == MediaStatus.Ready)
            return MediaErrors.NotReady("media.status");

        Status = MediaStatus.Failed;
        FailureCode = error.Code;
        FailureDescription = error.Description;
        UpdatedAt = now;
        return Result.Success;
    }

    /// <summary>Помечает объект на удаление. Удалением занимается фон</summary>
    public ErrorOr<Success> MarkDeleted(DateTime now)
    {
        if (Status == MediaStatus.Deleted)
            return Result.Success;

        Status = MediaStatus.Deleted;
        DeletedAt = now;
        UpdatedAt = now;
        return Result.Success;
    }

    private static bool HasExactVariantSet(IReadOnlyCollection<MediaVariant> variants) =>
        variants.Count == MediaConstraints.VariantsPerObject
        && variants.Select(variant => variant.Variant).Distinct().Count() == MediaConstraints.VariantsPerObject
        && variants.All(variant => variant.Variant is MediaVariantName.Thumb or MediaVariantName.Display);
}
