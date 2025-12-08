namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate;

public class FinancialTransactionType : SmartEnum<FinancialTransactionType>
{
    public static readonly FinancialTransactionType Income = new(nameof(Income), 1);
    public static readonly FinancialTransactionType Expense = new(nameof(Expense), 2);

    protected FinancialTransactionType(string name, int value)
        : base(name, value) { }
}
