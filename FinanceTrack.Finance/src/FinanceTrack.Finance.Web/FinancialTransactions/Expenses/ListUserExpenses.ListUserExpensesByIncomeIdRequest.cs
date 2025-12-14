namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class ListUserExpensesByIncomeIdRequest
{
    public const string Route = "/Transactions/Income/{IncomeTransactionId:guid}/Expenses";

    public Guid IncomeTransactionId { get; set; }
}

