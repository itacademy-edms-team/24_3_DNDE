namespace FinanceTrack.Finance.Core.WalletAggregate.Specifications;

public class WalletByIdSpec : Specification<Wallet>, ISingleResultSpecification<Wallet>
{
    public WalletByIdSpec(Guid walletId)
    {
        Query.Where(w => w.Id == walletId);
    }
}
