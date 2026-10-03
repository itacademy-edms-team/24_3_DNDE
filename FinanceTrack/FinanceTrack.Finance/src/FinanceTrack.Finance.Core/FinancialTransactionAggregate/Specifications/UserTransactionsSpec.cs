namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class UserTransactionsSpec : Specification<FinancialTransaction>
{
    public UserTransactionsSpec(
        string userId,
        DateOnly? from = null,
        DateOnly? to = null
    )
    {
        Query
            .Where(t => t.UserId == userId)
            .Where(t => from == null || t.OperationDate >= from)
            .Where(t => to == null || t.OperationDate <= to)
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAtUtc);
    }
}
