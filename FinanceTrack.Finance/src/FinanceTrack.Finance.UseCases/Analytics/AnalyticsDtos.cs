namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record OverviewAnalyticsDto(
    decimal TotalBalance,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetFlow,
    IReadOnlyList<AccountSummaryDto> Accounts
);

public sealed record AccountSummaryDto(
    Guid WalletId,
    string WalletName,
    string WalletType,
    decimal Balance,
    decimal Income,
    decimal Expense
);

public sealed record AccountAnalyticsDto(
    Guid WalletId,
    string WalletName,
    decimal Balance,
    decimal Income,
    decimal Expense,
    IReadOnlyList<CategoryBreakdownDto> IncomeByCategory,
    IReadOnlyList<CategoryBreakdownDto> ExpenseByCategory
);

public sealed record CategoryBreakdownDto(
    Guid? CategoryId,
    string? CategoryName,
    decimal Amount,
    decimal Percentage
);

public sealed record CashFlowDto(IReadOnlyList<CashFlowPeriodDto> Periods);

public sealed record CashFlowPeriodDto(
    int Year,
    int Month,
    decimal Income,
    decimal Expense,
    decimal Net
);

public sealed record SavingsProgressDto(
    IReadOnlyList<SavingsAccountProgressDto> Accounts
);

public sealed record SavingsAccountProgressDto(
    Guid WalletId,
    string WalletName,
    decimal Balance,
    decimal TargetAmount,
    DateOnly? TargetDate,
    decimal ProgressPercent
);

public sealed record CategoriesAnalyticsDto(
    IReadOnlyList<CategoryBreakdownDto> IncomeByCategory,
    IReadOnlyList<CategoryBreakdownDto> ExpenseByCategory
);