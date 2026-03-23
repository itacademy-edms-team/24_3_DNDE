namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

public sealed record CashFlowDto(IReadOnlyList<CashFlowPeriodDto> Periods);

public sealed record CashFlowPeriodDto(
    int Year,
    int Month,
    decimal Income,
    decimal Expense,
    decimal Net
);
