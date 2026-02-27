using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UseCases.Wallets.Create;

public sealed class CreateWalletHandler(
    IRepository<Wallet> _repo,
    IUnitOfWork _unitOfWork
) : ICommandHandler<CreateWalletCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateWalletCommand request,
        CancellationToken ct
    )
    {
        Wallet wallet;

        if (string.Equals(request.WalletType, nameof(WalletType.Savings), StringComparison.OrdinalIgnoreCase))
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

        await _repo.AddAsync(wallet, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(wallet.Id);
    }
}
