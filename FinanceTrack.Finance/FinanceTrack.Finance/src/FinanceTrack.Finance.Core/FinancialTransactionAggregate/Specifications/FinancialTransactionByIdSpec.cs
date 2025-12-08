namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class FinancialTransactionByIdSpec : Specification<FinancialTransaction>
{
    public FinancialTransactionByIdSpec(Guid transactionId) =>
        Query.Where(transaction => transaction.Id == transactionId);
}
