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

    /// <summary>Подпуть загрузок: инициализация и подтверждение.</summary>
    public const string Uploads = "/uploads";

    /// <summary>Подпуть подписи ссылок на чтение.</summary>
    public const string Links = "/links";
}
