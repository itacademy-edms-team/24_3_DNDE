namespace FinanceTrack.Finance.UseCases.Wallets;

public sealed record WalletDto(
    Guid Id,
    string Name,
    string WalletType,
    decimal Balance,
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate,
    bool IsArchived
);
