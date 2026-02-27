namespace FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

public class RecurringTransactionByIdSpec
    : Specification<RecurringTransaction>,
        ISingleResultSpecification<RecurringTransaction>
{
    public RecurringTransactionByIdSpec(Guid recurringId)
    {
        Query.Where(r => r.Id == recurringId);
    }
}

