namespace FinanceTrack.Finance.UseCases.Wallets.Update;

public sealed record UpdateWalletCommand(
    Guid WalletId,
    string UserId,
    string Name,
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate
) : ICommand<Result<WalletDto>>;
