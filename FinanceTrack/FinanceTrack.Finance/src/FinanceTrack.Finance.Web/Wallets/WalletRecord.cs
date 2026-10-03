namespace FinanceTrack.Finance.Web.Wallets;

public sealed record WalletRecord(
    Guid Id,
    string Name,
    string WalletType,
    decimal Balance,
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate,
    bool IsArchived
);
