namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class UpdateIncomeTransactionRequest
{
  public const string Route = "/Transactions/Income/{TransactionId}";
  public Guid TransactionId { get; set; }
  public decimal Amount { get; set; }
  public DateOnly OperationDate { get; set; }
  public bool IsMonthly { get; set; }
}
