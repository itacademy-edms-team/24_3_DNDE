namespace Sets.Application.Sets;

/// <summary>Тело POST /api/sets.</summary>
public sealed record CreateSetRequest(string? Title, string? Description);

/// <summary>
/// Тело PUT /api/sets/{id}.
/// </summary>
public sealed record UpdateSetRequest(string? Title, string? Description);

/// <summary>Источник Id: маршрут, а не тело запроса.</summary>
public static class SetRoute
{
    public const string Group = "/api/sets";
}
