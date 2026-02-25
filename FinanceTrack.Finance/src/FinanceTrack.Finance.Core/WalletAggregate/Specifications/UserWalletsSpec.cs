namespace FinanceTrack.Finance.Core.WalletAggregate.Specifications;

public class UserWalletsSpec : Specification<Wallet>
{
    public UserWalletsSpec(string userId)
    {
        Query
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.CreatedAtUtc);
    }
}
