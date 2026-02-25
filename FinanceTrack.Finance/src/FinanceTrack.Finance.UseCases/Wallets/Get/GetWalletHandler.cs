using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UseCases.Wallets.Get;

public sealed class GetWalletHandler(IReadRepository<Wallet> _repo)
    : IQueryHandler<GetWalletQuery, Result<WalletDto>>
{
    public async Task<Result<WalletDto>> Handle(GetWalletQuery request, CancellationToken ct)
    {
        var wallet = await _repo.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

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
