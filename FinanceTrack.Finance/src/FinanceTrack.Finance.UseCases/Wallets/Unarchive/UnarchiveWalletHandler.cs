using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Wallets.Unarchive;

public sealed class UnarchiveWalletHandler(IRepository<Wallet> _repo, IUnitOfWork _unitOfWork)
    : ICommandHandler<UnarchiveWalletCommand, Result>
{
    public async Task<Result> Handle(UnarchiveWalletCommand request, CancellationToken ct)
    {
        var spec = new WalletByIdSpec(request.WalletId);
        var wallet = await _repo.FirstOrDefaultAsync(spec, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        wallet.Unarchive();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
