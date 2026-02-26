using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Wallets.Archive;

public sealed class ArchiveWalletHandler(IRepository<Wallet> _repo)
    : ICommandHandler<ArchiveWalletCommand, Result>
{
    public async Task<Result> Handle(ArchiveWalletCommand request, CancellationToken ct)
    {
        var spec = new WalletByIdSpec(request.WalletId);
        var wallet = await _repo.GetByIdAsync(spec, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        wallet.Archive();
        await _repo.UpdateAsync(wallet, ct);
        return Result.Success();
    }
}
