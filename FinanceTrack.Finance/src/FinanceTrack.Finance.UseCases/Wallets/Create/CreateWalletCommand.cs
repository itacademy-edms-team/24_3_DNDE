namespace FinanceTrack.Finance.UseCases.Wallets.Create;

public sealed record CreateWalletCommand(
    string UserId,
    string Name,
    string WalletType, // "Checking" or "Savings"
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate
) : ICommand<Result<Guid>>;
