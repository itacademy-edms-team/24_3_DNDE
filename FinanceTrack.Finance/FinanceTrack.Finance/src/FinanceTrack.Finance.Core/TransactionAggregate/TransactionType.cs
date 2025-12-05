namespace FinanceTrack.Finance.Core.TransactionAggregate;

public class TransactionType : SmartEnum<TransactionType>
{
    public static readonly TransactionType Income = new(nameof(Income), 1);
    public static readonly TransactionType Expense = new(nameof(Expense), 2);

    protected TransactionType(string name, int value)
        : base(name, value) { }
}
