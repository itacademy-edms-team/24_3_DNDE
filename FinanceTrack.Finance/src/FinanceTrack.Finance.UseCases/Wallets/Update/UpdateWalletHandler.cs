using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UseCases.Wallets.Update;

public sealed class UpdateWalletHandler(IRepository<Wallet> _repo)
    : ICommandHandler<UpdateWalletCommand, Result<WalletDto>>
{
    public async Task<Result<WalletDto>> Handle(
        UpdateWalletCommand request,
        CancellationToken ct
    )
    {
        var wallet = await _repo.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        wallet
            .UpdateName(request.Name)
            .SetAllowNegativeBalance(request.AllowNegativeBalance)
            .UpdateTarget(request.TargetAmount, request.TargetDate);

        await _repo.UpdateAsync(wallet, ct);

        return Result.Success(
            new WalletDto(
                wallet.Id,
                wallet.Name,
                wallet.WalletType.Name,
                wallet.Balance,
                wallet.AllowNegativeBalance,
                wallet.TargetAmount,
                wallet.TargetDate,
                wallet.IsArchived
            )
        );
    }
}
