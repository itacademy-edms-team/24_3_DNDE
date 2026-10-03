using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.Services;

public class CreateWalletService(IRepository<Wallet> walletRepo)
{
    public async Task<Result<Guid>> Execute(
        CreateWalletRequest request,
        CancellationToken cancel = default
    )
    {
        Wallet wallet;

        if (
            string.Equals(
                request.WalletType,
                nameof(WalletType.Savings),
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            if (!request.TargetAmount.HasValue || request.TargetAmount.Value <= 0)
                return Result.Error("TargetAmount is required for Savings wallets.");

            wallet = Wallet.CreateSavings(
                request.UserId,
                request.Name,
                request.TargetAmount.Value,
                request.TargetDate,
                request.AllowNegativeBalance
            );
        }
        else
        {
            wallet = Wallet.CreateChecking(
                request.UserId,
                request.Name,
                request.AllowNegativeBalance
            );
        }

        await walletRepo.AddAsync(wallet, cancel);
        return Result.Success(wallet.Id);
    }
}
