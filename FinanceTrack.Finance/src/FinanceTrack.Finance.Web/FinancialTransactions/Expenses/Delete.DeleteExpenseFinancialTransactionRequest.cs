namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class DeleteExpenseTransactionRequest
{
    public const string Route = "/Transactions/Expense/{transactionId:guid}";
    public Guid TransactionId { get; set; }
}

