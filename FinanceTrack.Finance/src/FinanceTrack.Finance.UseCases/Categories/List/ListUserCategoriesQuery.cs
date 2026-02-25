namespace FinanceTrack.Finance.UseCases.Categories.List;

public sealed record ListUserCategoriesQuery(string UserId) : IQuery<IReadOnlyList<CategoryDto>>;
