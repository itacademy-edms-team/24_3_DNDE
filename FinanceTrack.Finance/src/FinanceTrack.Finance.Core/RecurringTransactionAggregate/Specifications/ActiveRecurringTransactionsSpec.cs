namespace FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

public class ActiveRecurringTransactionsSpec : Specification<RecurringTransaction>
{
    public ActiveRecurringTransactionsSpec()
    {
        Query.Where(r => r.IsActive && (r.EndDate == null || r.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}
