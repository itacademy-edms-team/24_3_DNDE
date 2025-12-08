namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class UserIncomeFinancialTransactionsSpec : Specification<FinancialTransaction>
{
    public UserIncomeFinancialTransactionsSpec(string userId)
    {
        Query
            .Where(t => t.UserId == userId && t.TransactionType == FinancialTransactionType.Income)
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAtUtc);
    }
}
