namespace FinanceTrack.Finance.UseCases.Categories.List;

public sealed record ListUserCategoriesQuery(string UserId, string? Type = null) : IQuery<IReadOnlyList<CategoryDto>>;
