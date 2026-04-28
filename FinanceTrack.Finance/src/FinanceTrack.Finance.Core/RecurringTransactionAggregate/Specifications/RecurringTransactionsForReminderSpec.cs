namespace FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

public class RecurringTransactionsForReminderSpec : Specification<RecurringTransaction>
{
    public RecurringTransactionsForReminderSpec(DateOnly today)
    {
        Query
            .Where(rt => rt.IsActive)
            .Where(rt => rt.StartDate <= today)
            .Where(rt => rt.EndDate == null || rt.EndDate >= today)
            .Where(rt => rt.LastEmailReminderDate == null)
            .Include(rt => rt.Wallet);
    }
}
