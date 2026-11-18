using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <summary>Карточка набора в списке «Мои наборы».</summary>
public sealed record SetSummaryResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static SetSummaryResponse From(Set set) =>
        new(set.Id, set.Title.Value, set.Description.Value, set.CreatedAt, set.UpdatedAt);
}

/// <summary>Набор целиком, без элементов: элементы приходят отдельным запросом, их число - в ElementCount.</summary>
public sealed record SetResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ElementCount)
{
    public static SetResponse From(Set set, int elementCount) =>
        new(set.Id, set.Title.Value, set.Description.Value, set.CreatedAt, set.UpdatedAt, elementCount);
}
