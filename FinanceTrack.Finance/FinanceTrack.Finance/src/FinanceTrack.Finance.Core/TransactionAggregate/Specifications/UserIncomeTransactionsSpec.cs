namespace FinanceTrack.Finance.Core.TransactionAggregate.Specifications;

public class UserIncomeTransactionsSpec : Specification<Transaction>
{
    public UserIncomeTransactionsSpec(string userId)
    {
        Query
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income)
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAtUtc);
    }
}
