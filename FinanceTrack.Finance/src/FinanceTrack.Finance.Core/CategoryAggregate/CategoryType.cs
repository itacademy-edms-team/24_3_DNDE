namespace FinanceTrack.Finance.Core.CategoryAggregate;

public class CategoryType : SmartEnum<CategoryType>
{
    public static readonly CategoryType Income = new(nameof(Income), 1);
    public static readonly CategoryType Expense = new(nameof(Expense), 2);

    protected CategoryType(string name, int value)
        : base(name, value) { }
}
