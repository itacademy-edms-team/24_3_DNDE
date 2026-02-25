namespace FinanceTrack.Finance.Core.RecurringTransactionAggregate;

public class RecurringTransactionType : SmartEnum<RecurringTransactionType>
{
    public static readonly RecurringTransactionType Income = new(nameof(Income), 1);
    public static readonly RecurringTransactionType Expense = new(nameof(Expense), 2);

    protected RecurringTransactionType(string name, int value)
        : base(name, value) { }
}
