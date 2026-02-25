namespace FinanceTrack.Finance.Core.WalletAggregate.Specifications;

public class UserActiveWalletsSpec : Specification<Wallet>
{
    public UserActiveWalletsSpec(string userId)
    {
        Query
            .Where(w => w.UserId == userId && !w.IsArchived)
            .OrderBy(w => w.CreatedAtUtc);
    }
}
