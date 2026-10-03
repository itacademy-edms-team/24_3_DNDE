namespace FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

public class UserRecurringTransactionsSpec : Specification<RecurringTransaction>
{
    public UserRecurringTransactionsSpec(string userId)
    {
        Query
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc);
    }
}
