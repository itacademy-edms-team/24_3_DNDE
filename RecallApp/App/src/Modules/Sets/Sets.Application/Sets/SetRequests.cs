namespace Sets.Application.Sets;

/// <summary>
/// Представляет тело запроса на создание набора.
/// </summary>
/// <param name="Title">Название набора; обязательно.</param>
/// <param name="Description">Описание; <see langword="null"/> и пробельная строка
/// означают отсутствие описания.</param>
public sealed record CreateSetRequest(string? Title, string? Description);

/// <summary>
/// Представляет тело запроса на полную замену полей набора.
/// </summary>
/// <param name="Title">Новое название; обязательно.</param>
/// <param name="Description">Новое описание; <see langword="null"/> и пробельная строка
/// означают отсутствие описания.</param>
public sealed record UpdateSetRequest(string? Title, string? Description);

/// <summary>
/// Предоставляет маршруты наборов.
/// </summary>
public static class SetRoute
{
    public const string Group = "/api/sets";
}
