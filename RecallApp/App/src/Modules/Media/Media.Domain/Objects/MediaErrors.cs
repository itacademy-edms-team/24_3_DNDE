using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Коды ошибок модуля Media - публичный контракт клиента (ADR 0031/0038):
/// по коду клиент выбирает текст, описание в интерфейс не попадает.
/// </summary>
public static class MediaErrors
{
    /// <summary>Личность не дошла до модуля: провижининг не отработал, запрос нельзя связать с владельцем.</summary>
    public static Error UserUnresolved =>
        Error.Unauthorized("Media.UserUnresolved", "Не удалось определить пользователя.");

    /// <summary>У пользователя слишком много незавершённых загрузок: сначала нужно подтвердить имеющиеся или дождаться чистки.</summary>
    public static Error PendingLimitReached =>
        Error.Conflict("Media.PendingLimitReached", "Слишком много незавершённых загрузок.",
            new Dictionary<string, object> { ["maxPending"] = MediaConstraints.MaxPendingPerOwner });

    /// <summary>Ключ объекта не соответствует формату: ошибка внутреннего состояния, а не ввода.</summary>
    public static Error ObjectKeyInvalid =>
        Error.Validation("Media.ObjectKeyInvalid", "Некорректный ключ объекта.");

    /// <summary>Ключ слота пуст или длиннее допустимого.</summary>
    public static Error ReferenceInvalid =>
        Error.Validation("Media.ReferenceInvalid", "Некорректный ключ ссылки.");

    /// <summary>Сообщение outbox не разбирается или нарушает контракт синхронизации.</summary>
    public static Error MessageInvalid =>
        Error.Validation("Media.MessageInvalid", "Некорректное сообщение синхронизации ссылок.");

    /// <summary>Файла нет, он чужой или удалён — состояния неразличимы, чтобы не выдавать существование.</summary>
    public static Error NotFound => Error.NotFound("Media.NotFound", "Изображение не найдено.");

    /// <summary>Файл существует, но не готов к использованию: `pending`, `failed` или `deleted`.</summary>
    public static Error NotReady(string target) =>
        Error.Conflict("Media.NotReady", "Изображение ещё не готово.", new Dictionary<string, object> { ["target"] = target });

    /// <summary>Файл используется: удаление запрещено, пока на него есть ссылки.</summary>
    public static Error InUse => Error.Conflict("Media.InUse", "Изображение используется и не может быть удалено.");

    /// <summary>Окно подтверждения истекло: политика загрузки больше не действует.</summary>
    public static Error UploadExpired =>
        Error.Validation("Media.UploadExpired", "Срок подтверждения загрузки истёк.");

    /// <summary>Объекта в хранилище нет: загрузка не состоялась или объект уже удалён.</summary>
    public static Error ObjectMissing =>
        Error.Validation("Media.ObjectMissing", "Файл не найден в хранилище.");

    /// <summary>Заявленный тип не входит в белый список.</summary>
    public static Error ContentTypeUnsupported =>
        Error.Validation("Media.ContentTypeUnsupported", "Этот тип файла не поддерживается.");

    /// <summary>Размер больше допустимого.</summary>
    public static Error FileTooLarge =>
        Error.Validation("Media.FileTooLarge", "Файл слишком большой.",
            new Dictionary<string, object> { ["maxSizeBytes"] = MediaConstraints.MaxUploadBytes });

    /// <summary>Сигнатура файла не соответствует ни одному разрешённому формату.</summary>
    public static Error InvalidFormat =>
        Error.Validation("Media.InvalidFormat", "Файл не является изображением PNG, JPEG или WebP.");

    /// <summary>Анимированные изображения не поддерживаются.</summary>
    public static Error UnsupportedAnimation =>
        Error.Validation("Media.UnsupportedAnimation", "Анимированные изображения не поддерживаются.");

    /// <summary>Слишком большая картинка: число пикселей или сторона больше лимита.</summary>
    public static Error ResolutionTooLarge =>
        Error.Validation("Media.ResolutionTooLarge", "Разрешение изображения слишком велико.",
            new Dictionary<string, object>
            {
                ["maxPixels"] = MediaConstraints.MaxPixels,
                ["maxSide"] = MediaConstraints.MaxSide,
            });

    /// <summary>Файл прошёл проверку сигнатуры, но не декодируется.</summary>
    public static Error ImageCorrupt =>
        Error.Validation("Media.ImageCorrupt", "Файл повреждён и не может быть обработан.");

    /// <summary>Запрошено больше идентификаторов, чем разрешено подписывать за один раз.</summary>
    public static Error LinksLimitExceeded(int requested) =>
        Error.Validation("Media.LinksLimitExceeded", $"Запрошено {requested} ссылок при лимите {MediaConstraints.MaxLinksPerRequest}.",
            new Dictionary<string, object> { ["max"] = MediaConstraints.MaxLinksPerRequest });
}
