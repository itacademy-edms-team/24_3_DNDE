namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class UpdateExpenseTransactionRequest
{
    public const string Route = "/Transactions/Expense/{TransactionId:guid}";

    public Guid TransactionId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
    public bool IsMonthly { get; set; }
    public Guid IncomeTransactionId { get; set; }
}

