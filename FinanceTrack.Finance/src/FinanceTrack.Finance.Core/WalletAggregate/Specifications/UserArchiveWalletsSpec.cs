using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.WalletAggregate.Specifications;

public class UserArchiveWalletsSpec : Specification<Wallet>
{
    public UserArchiveWalletsSpec(string userId)
    {
        Query.Where(w => w.UserId == userId && w.IsArchived).OrderBy(w => w.CreatedAtUtc);
    }
}
