namespace Sets.Application.Sets;

/// <summary>Тело POST /api/sets.</summary>
/// <param name="Title">Название набора.</param>
/// <param name="Description">Описание; null трактуется как пустая строка.</param>
public sealed record CreateSetRequest(string? Title, string? Description);

/// <summary>
/// Тело PUT /api/sets/{id}.
/// </summary>
/// <param name="Title">Новое название.</param>
/// <param name="Description">Новое описание; null трактуется как пустая строка.</param>
public sealed record UpdateSetRequest(string? Title, string? Description);

/// <summary>Источник Id: маршрут, а не тело запроса.</summary>
public static class SetRoute
{
    public const string Group = "/api/sets";
}
