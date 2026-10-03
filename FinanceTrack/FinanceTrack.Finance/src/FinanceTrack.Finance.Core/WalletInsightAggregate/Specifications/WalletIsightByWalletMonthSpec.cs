namespace FinanceTrack.Finance.Core.WalletInsightAggregate.Specifications;

public class WalletInsightByWalletMonthSpec
    : Specification<WalletInsight>,
        ISingleResultSpecification<WalletInsight>
{
    public WalletInsightByWalletMonthSpec(string userId, Guid walletId, DateOnly insightMonth)
    {
        Query.Where(i =>
            i.UserId == userId && i.WalletId == walletId && i.InsightMonth == insightMonth
        );
    }
}
