using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UseCases.Wallets.Archive;

public sealed class ArchiveWalletHandler(IRepository<Wallet> _repo)
    : ICommandHandler<ArchiveWalletCommand, Result>
{
    public async Task<Result> Handle(ArchiveWalletCommand request, CancellationToken ct)
    {
        var wallet = await _repo.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        wallet.Archive();
        await _repo.UpdateAsync(wallet, ct);
        return Result.Success();
    }
}
