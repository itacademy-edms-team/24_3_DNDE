namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

public sealed record CategoryBreakdownDto(
    Guid? CategoryId,
    string? CategoryName,
    decimal Amount,
    decimal Percentage
);
