namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.List;

public interface IListUserIncomeTransactionsQueryService
{
    Task<Result<IReadOnlyList<TransactionDto>>> GetUserIncomeTransactions(
        string userId,
        CancellationToken cancellationToken = default
    );
}
