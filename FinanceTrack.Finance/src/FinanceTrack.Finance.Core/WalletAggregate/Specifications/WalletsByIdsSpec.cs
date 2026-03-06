using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.WalletAggregate.Specifications;

public class WalletsByIdsSpec : Specification<Wallet>
{
    public WalletsByIdsSpec(IEnumerable<Guid> walletIds)
    {
        Query.Where(w => walletIds.Contains(w.Id));
    }
}
