namespace FinanceTrack.Finance.Core.TransactionAggregate.Specifications;

public class TransactionByIdSpec : Specification<Transaction>
{
    public TransactionByIdSpec(Guid transactionId) =>
        Query.Where(transaction => transaction.Id == transactionId);
}
