namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

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
