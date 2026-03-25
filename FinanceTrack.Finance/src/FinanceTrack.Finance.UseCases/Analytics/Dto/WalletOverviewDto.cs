namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

public sealed record WalletOverviewDto(
    Guid WalletId,
    string WalletName,
    decimal Balance,
    decimal Income,
    decimal Expense,
    decimal NetFlow
);
