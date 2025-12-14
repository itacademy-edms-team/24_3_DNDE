namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class CreateIncomeTransactionRequest
{
    public const string Route = "/Transactions/Income";
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
    public bool IsMonthly { get; set; }
}
