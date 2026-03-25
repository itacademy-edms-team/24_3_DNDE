namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

public sealed record CategoriesAnalyticsDto(
    IReadOnlyList<CategoryBreakdownDto> IncomeByCategory,
    IReadOnlyList<CategoryBreakdownDto> ExpenseByCategory
);
