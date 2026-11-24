using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Представляет медиа-объект - корень агрегата: владельца, состояние жизненного цикла,
/// метаданные исходника и готовые варианты.
/// </summary>
/// <remarks>
/// <para>Байты лежат в объектном хранилище, здесь только метаданные (ADR 0029, ADR 0037).</para>
/// <para>Жизненный цикл однонаправленный:
/// <c>pending</c> -> <c>ready</c> | <c>failed</c>, далее только <c>deleted</c>.
/// Готовый объект нельзя перевести в <c>failed</c>, а удалённый - в любое другое состояние.</para>
/// </remarks>
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

    /// <summary>Возвращает идентификатор медиа-объекта.</summary>
    public Guid Id { get; private set; }

    /// <summary>Возвращает идентификатор владельца.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Возвращает текущее состояние жизненного цикла.</summary>
    public MediaStatus Status { get; private set; }

    /// <summary>
    /// Возвращает тип, который заявил клиент при инициализации загрузки.
    /// </summary>
    /// <remarks>
    /// Значение проверяется только на непустоту:
    /// соответствие реальному содержимому подтверждается после загрузки по сигнатуре файла, а не по заголовку.
    /// </remarks>
    public string DeclaredContentType { get; private set; } = null!;
    
    /// <summary>
    /// Возвращает формат исходника, определённый по сигнатуре файла; <see langword="null"/> до готовности.
    /// </summary>
    public MediaFormat? SourceFormat { get; private set; }

    /// <summary>
    /// Возвращает размер исходника в байтах; <see langword="null"/> до готовности.
    /// </summary>
    public long? SourceSizeBytes { get; private set; }

    /// <summary>
    /// Возвращает ширину исходника в пикселях; <see langword="null"/> до готовности.
    /// </summary>
    public int? Width { get; private set; }

    /// <summary>
    /// Возвращает высоту исходника в пикселях; <see langword="null"/> до готовности.
    /// </summary>
    public int? Height { get; private set; }

    /// <summary>
    /// Возвращает код причины отказа; <see langword="null"/>, если объект не отвергнут.
    /// </summary>
    public string? FailureCode { get; private set; }

    /// <summary>
    /// Возвращает описание причины отказа для логов; <see langword="null"/>, если объект не отвергнут.
    /// </summary>
    public string? FailureDescription { get; private set; }

    /// <summary>
    /// Возвращает момент окончания окна доверия к загрузке в UTC: позже подтверждать уже нечего.
    /// </summary>
    public DateTime UploadExpiresAt { get; private set; }

    /// <summary>
    /// Возвращает момент создания записи в UTC.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент последнего изменения состояния в UTC.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент перехода в <c>ready</c> в UTC; <see langword="null"/>, пока объект не готов.
    /// </summary>
    public DateTime? ReadyAt { get; private set; }

    /// <summary>
    /// Возвращает момент пометки на удаление в UTC; <see langword="null"/>, пока объект не удалён.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Возвращает готовые варианты изображения.
    /// </summary>
    /// <remarks>
    /// Коллекция пуста до перехода в <c>ready</c>:
    /// варианты фиксируются только вместе с метаданными исходника.
    /// </remarks>
    public IReadOnlyCollection<MediaVariant> Variants => _variants;

    /// <summary>
    /// Создаёт запись в состоянии <c>pending</c> и открывает окно загрузки.
    /// </summary>
    /// <param name="ownerId">Владелец будущего объекта.</param>
    /// <param name="declaredContentType">Тип, заявленный клиентом; нормализуется.</param>
    /// <param name="now">Текущий момент в UTC; передаёт вызывающий.</param>
    /// <returns>
    /// Новый объект либо ошибка:
    /// <see cref="MediaErrors.NotFound"/> (для пустого владельца)
    /// или <see cref="MediaErrors.ContentTypeUnsupported"/>.
    /// </returns>
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

    /// <summary>
    /// Проверяет, что файл ещё можно подтверждать.
    /// </summary>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <returns>Успех либо ошибка:
    /// <see cref="MediaErrors.NotFound"/> (объект удалён),
    /// <see cref="MediaErrors.ObjectMissing"/> (уже не <c>pending</c>)
    /// или <see cref="MediaErrors.UploadExpired"/>.
    /// </returns>
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
    /// Переводит объект в <c>ready</c>: фиксирует метаданные исходника и варианты изображений.
    /// </summary>
    /// <remarks>
    /// Проверки сосредоточены здесь, чтобы никакой вызов не мог записать полуготовый файл.
    /// </remarks>
    /// <param name="format">Формат, определённый по сигнатуре.</param>
    /// <param name="width">Ширина исходника в пикселях.</param>
    /// <param name="height">Высота исходника в пикселях.</param>
    /// <param name="sourceSizeBytes">Размер исходника в байтах.</param>
    /// <param name="variants">Варианты: <c>thumb</c> и <c>display</c>.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <returns>
    /// Успех либо ошибка: <see cref="MediaErrors.ObjectMissing"/>,
    /// <see cref="MediaErrors.ImageCorrupt"/>, <see cref="MediaErrors.FileTooLarge"/>
    /// или <see cref="MediaErrors.ResolutionTooLarge"/>.
    /// </returns>
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

    /// <summary>
    /// Переводит объект в <c>failed</c>.
    /// </summary>
    /// <param name="error">Причина отказа; её код и описание сохраняются в объекте.</param>
    /// <param name="now">Текущий момент в UTC; задаётся вызывающим</param>
    /// <returns>
    /// Успех либо ошибка:
    /// <see cref="MediaErrors.NotFound"/> (объект удалён)
    /// или <see cref="MediaErrors.NotReady"/> (объект уже готов).
    /// </returns>
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

    /// <summary>
    /// Помечает объект на удаление.
    /// </summary>
    /// <remarks>
    /// Повторный вызов ничего не меняет.
    /// Байты и варианты удаляет фоновая чистка модуля, а не этот метод.
    /// </remarks>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <returns>Всегда успех.</returns>
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
