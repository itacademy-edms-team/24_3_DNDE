namespace Media.Application.Objects;

/// <summary>
/// Предоставляет маршруты модуля Media.
/// </summary>
/// <remarks>
/// Подпути относительны и применяются к <see cref="Group"/>.
/// </remarks>
public static class MediaRoute
{
    /// <summary>Базовый префикс всех маршрутов модуля.</summary>
    public const string Group = "/api/media";

    /// <summary>Коллекция загрузок: инициализация.</summary>
    public const string Uploads = "/uploads";

    /// <summary>Подтверждение загрузки по идентификатору файла.</summary>
    public const string ConfirmUpload = Uploads + "/{mediaId:guid}/confirm";

    /// <summary>Одиночный файл по идентификатору.</summary>
    public const string MediaItem = "/{mediaId:guid}";

    /// <summary>Коллекция подписи ссылок на чтение.</summary>
    public const string Links = "/links";
}
