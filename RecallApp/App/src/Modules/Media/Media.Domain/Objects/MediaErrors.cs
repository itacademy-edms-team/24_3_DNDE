using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Предоставляет каталог ошибок домена Media.
/// </summary>
/// <remarks>
/// Код ошибки является частью контракта для клиента (ADR 0031, ADR 0038)
/// и имеет формат <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>: по нему выбирается текст для пользователя.
/// Описание ошибки предназначено для логов и в интерфейс не попадает.
/// Метаданные используются в логах и внутренних обработчиках;
/// в ответ ProblemDetails сейчас уходит только <c>target</c>.
/// </remarks>
public static class MediaErrors
{
    /// <summary>
    /// Личность не сопоставлена доменной строке.
    /// </summary>
    /// <remarks>JIT-провижининг не выполнен (ADR 0020).</remarks>
    public static readonly Error UserUnresolved = Error.Unauthorized(
        code: "Media.UserUnresolved",
        description: "Domain user is not resolved for the current request.");

    /// <summary>
    /// У пользователя слишком много незавершённых загрузок.
    /// </summary>
    /// <remarks>Сначала нужно подтвердить имеющиеся или дождаться чистки.</remarks>
    public static readonly Error PendingLimitReached = Error.Conflict(
        code: "Media.PendingLimitReached",
        description: "Too many pending uploads.",
        metadata: new Dictionary<string, object> { ["maxPending"] = MediaConstraints.MaxPendingPerOwner });

    /// <summary>
    /// Ключ слота ссылки пуст или длиннее допустимого.
    /// </summary>
    public static readonly Error ReferenceInvalid = Error.Validation(
        code: "Media.ReferenceInvalid",
        description: "Reference slot key is malformed.");

    /// <summary>
    /// Файла нет, он чужой или удалён.
    /// </summary>
    /// <remarks>Состояния неразличимы, чтобы не выдавать существование чужого файла.</remarks>
    public static readonly Error NotFound = Error.NotFound(
        code: "Media.NotFound",
        description: "Image does not exist or is not owned by the current user.");

    /// <summary>
    /// Создаёт ошибку «файл существует, но не готов к использованию».
    /// </summary>
    /// <param name="target">Имя поля, значения которого ждал вызывающий.</param>
    /// <returns>
    /// Ошибка конфликта с кодом <c>Media.NotReady</c> и именем поля в метаданных.
    /// </returns>
    public static Error NotReady(string target) => Error.Conflict(
        code: "Media.NotReady",
        description: "Image is not ready yet.",
        metadata: new Dictionary<string, object> { ["target"] = target });

    /// <summary>
    /// Файл используется: на него есть ссылки.
    /// </summary>
    /// <remarks>Удаление запрещено, пока ссылки не убраны.</remarks>
    public static readonly Error InUse = Error.Conflict(
        code: "Media.InUse",
        description: "Image is referenced and cannot be deleted.");

    /// <summary>
    /// Окно подтверждения истекло.
    /// </summary>
    /// <remarks>Политика загрузки больше не действует.</remarks>
    public static readonly Error UploadExpired = Error.Validation(
        code: "Media.UploadExpired",
        description: "Upload confirmation window has expired.");

    /// <summary>
    /// Объекта в хранилище нет.
    /// </summary>
    /// <remarks>Загрузка не состоялась или объект уже удалён.</remarks>
    public static readonly Error ObjectMissing = Error.Validation(
        code: "Media.ObjectMissing",
        description: "File is missing from object storage.");

    /// <summary>
    /// Заявленный тип не входит в белый список.
    /// </summary>
    public static readonly Error ContentTypeUnsupported = Error.Validation(
        code: "Media.ContentTypeUnsupported",
        description: "Content type is not supported.");

    /// <summary>
    /// Размер файла больше допустимого.
    /// </summary>
    public static readonly Error FileTooLarge = Error.Validation(
        code: "Media.FileTooLarge",
        description: "File size exceeds the limit.",
        metadata: new Dictionary<string, object> { ["maxSizeBytes"] = MediaConstraints.MaxUploadBytes });

    /// <summary>
    /// Сигнатура файла не соответствует ни одному разрешённому формату.
    /// </summary>
    public static readonly Error InvalidFormat = Error.Validation(
        code: "Media.InvalidFormat",
        description: "File is not a PNG, JPEG or WebP image.");

    /// <summary>
    /// Анимированные изображения не поддерживаются.
    /// </summary>
    public static readonly Error UnsupportedAnimation = Error.Validation(
        code: "Media.UnsupportedAnimation",
        description: "Animated images are not supported.");

    /// <summary>
    /// Разрешение изображения больше допустимого.
    /// </summary>
    /// <remarks>Ограничиваются и число пикселей, и длина стороны.</remarks>
    public static readonly Error ResolutionTooLarge = Error.Validation(
        code: "Media.ResolutionTooLarge",
        description: "Image resolution exceeds the limit.",
        metadata: new Dictionary<string, object>
        {
            ["maxPixels"] = MediaConstraints.MaxPixels,
            ["maxSide"] = MediaConstraints.MaxSide,
        });

    /// <summary>
    /// Файл прошёл проверку сигнатуры, но не декодируется.
    /// </summary>
    public static readonly Error ImageCorrupt = Error.Validation(
        code: "Media.ImageCorrupt",
        description: "File is corrupt and cannot be processed.");

    /// <summary>
    /// Создаёт ошибку превышения лимита идентификаторов в одном запросе.
    /// </summary>
    /// <param name="requested">Сколько идентификаторов запросил клиент.</param>
    /// <returns>
    /// Ошибка валидации с кодом <c>Media.LinksLimitExceeded</c>
    /// и запрошенным и максимальным числом идентификаторов в метаданных.
    /// </returns>
    public static Error LinksLimitExceeded(int requested) => Error.Validation(
        code: "Media.LinksLimitExceeded",
        description: $"Requested {requested} links, limit is {MediaConstraints.MaxLinksPerRequest}.",
        metadata: new Dictionary<string, object>
        {
            ["requested"] = requested,
            ["max"] = MediaConstraints.MaxLinksPerRequest,
        });
}
