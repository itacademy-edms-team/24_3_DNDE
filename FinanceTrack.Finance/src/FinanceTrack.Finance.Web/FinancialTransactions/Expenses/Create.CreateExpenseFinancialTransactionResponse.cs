namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class CreateExpenseTransactionResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

