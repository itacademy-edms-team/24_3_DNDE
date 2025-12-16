namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class UserExpenseFinancialTransactionsByIncomeSpec : Specification<FinancialTransaction>
{
    public UserExpenseFinancialTransactionsByIncomeSpec(string userId, Guid incomeTransactionId)
    {
        Query
            .Where(
                t =>
                    t.UserId == userId
                    && t.TransactionType == FinancialTransactionType.Expense
                    && t.IncomeTransactionId == incomeTransactionId
            )
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAtUtc);
    }
}

