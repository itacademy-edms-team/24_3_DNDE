using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Wallets.Update;

public sealed class UpdateWalletHandler(IRepository<Wallet> _repo, IUnitOfWork _unitOfWork)
    : ICommandHandler<UpdateWalletCommand, Result<WalletDto>>
{
    public async Task<Result<WalletDto>> Handle(UpdateWalletCommand request, CancellationToken ct)
    {
        var spec = new WalletByIdSpec(request.WalletId);
        var wallet = await _repo.FirstOrDefaultAsync(spec, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        wallet
            .UpdateName(request.Name)
            .SetAllowNegativeBalance(request.AllowNegativeBalance)
            .UpdateTarget(request.TargetAmount, request.TargetDate);

        await _unitOfWork.SaveChangesAsync(ct);

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
