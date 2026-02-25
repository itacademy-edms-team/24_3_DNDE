namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class TransactionByRelatedIdSpec
    : Specification<FinancialTransaction>,
        ISingleResultSpecification<FinancialTransaction>
{
    public TransactionByRelatedIdSpec(Guid relatedTransactionId)
    {
        Query.Where(t => t.RelatedTransactionId == relatedTransactionId);
    }
}
