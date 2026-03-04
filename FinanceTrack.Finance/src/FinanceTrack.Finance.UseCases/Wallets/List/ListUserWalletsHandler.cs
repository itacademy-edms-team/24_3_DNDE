using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed class ListUserWalletsHandler(IReadRepository<Wallet> repo)
    : IQueryHandler<ListUserWalletsQuery, IReadOnlyList<WalletDto>>
{
    public async Task<IReadOnlyList<WalletDto>> Handle(
        ListUserWalletsQuery request,
        CancellationToken ct
    )
    {
        var spec = new UserActiveWalletsSpec(request.UserId);
        var wallets = await repo.ListAsync(spec, ct);

        return wallets
            .Select(w => new WalletDto(
                w.Id,
                w.Name,
                w.WalletType.Name,
                w.Balance,
                w.AllowNegativeBalance,
                w.TargetAmount,
                w.TargetDate,
                w.IsArchived
            ))
            .ToList();
    }
}
