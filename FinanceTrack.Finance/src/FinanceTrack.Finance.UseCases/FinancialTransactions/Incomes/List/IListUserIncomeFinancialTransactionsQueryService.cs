namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.List;

public interface IListUserIncomeFinancialTransactionsQueryService
{
    Task<Result<IReadOnlyList<FinancialTransactionDto>>> GetUserIncomeTransactions(
        string userId,
        CancellationToken cancellationToken = default
    );
}
