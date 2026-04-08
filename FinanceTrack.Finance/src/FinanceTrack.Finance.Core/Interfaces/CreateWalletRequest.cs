namespace FinanceTrack.Finance.Core.Interfaces;

public record CreateWalletRequest(
    string UserId,
    string Name,
    string WalletType,
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate
);
