namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class CreateExpenseTransactionRequest
{
    public const string Route = "/Transactions/Expense";

    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
    public bool IsMonthly { get; set; }
    public Guid IncomeTransactionId { get; set; }
}

