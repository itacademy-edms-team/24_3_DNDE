namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;

public interface IListUserExpenseFinancialTransactionsQueryService
{
    Task<Result<IReadOnlyList<FinancialTransactionDto>>> GetUserExpenseTransactions(
        string userId,
        Guid incomeTransactionId,
        CancellationToken cancellationToken = default
    );
}

