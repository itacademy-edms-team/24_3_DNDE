using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed class ListUserArchiveWalletsHandler(IRepository<Wallet> repo)
    : IQueryHandler<ListUserArchiveWalletsQuery, IReadOnlyList<WalletDto>>
{
    public async Task<IReadOnlyList<WalletDto>> Handle(
        ListUserArchiveWalletsQuery request,
        CancellationToken ct
    )
    {
        var spec = new UserArchiveWalletsSpec(request.userId);
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
