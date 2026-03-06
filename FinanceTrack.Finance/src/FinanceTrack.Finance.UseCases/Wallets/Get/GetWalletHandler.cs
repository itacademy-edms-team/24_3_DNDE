using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Wallets.Get;

public sealed class GetWalletHandler(IReadRepository<Wallet> repo)
    : IQueryHandler<GetWalletQuery, Result<WalletDto>>
{
    public async Task<Result<WalletDto>> Handle(GetWalletQuery request, CancellationToken ct)
    {
        var spec = new WalletByIdSpec(request.WalletId);
        var wallet = await repo.FirstOrDefaultAsync(spec, ct);
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
