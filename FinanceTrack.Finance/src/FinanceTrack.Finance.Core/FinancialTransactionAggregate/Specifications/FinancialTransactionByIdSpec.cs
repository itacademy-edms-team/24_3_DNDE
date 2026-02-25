namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class FinancialTransactionByIdSpec
    : Specification<FinancialTransaction>,
        ISingleResultSpecification<FinancialTransaction>
{
    public FinancialTransactionByIdSpec(Guid transactionId)
    {
        Query.Where(t => t.Id == transactionId);
    }
}
