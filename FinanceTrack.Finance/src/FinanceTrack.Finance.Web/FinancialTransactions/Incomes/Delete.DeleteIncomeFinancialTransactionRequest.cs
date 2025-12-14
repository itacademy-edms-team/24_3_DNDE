namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class DeleteIncomeTransactionRequest
{
    public const string Route = "/Transactions/Income/{transactionId:guid}";
    public Guid TransactionId { get; set; }
}
