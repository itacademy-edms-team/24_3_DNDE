using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Wallets.Create;

public sealed record CreateWalletCommand(
    string UserId,
    string Name,
    string WalletType,
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate
)
    : CreateWalletRequest(UserId, Name, WalletType, AllowNegativeBalance, TargetAmount, TargetDate),
        ICommand<Result<Guid>>;
