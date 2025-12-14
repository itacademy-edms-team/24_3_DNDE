namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;

public sealed class ListUserExpenseFinancialTransactionsHandler(
    IListUserExpenseFinancialTransactionsQueryService _service
)
    : IQueryHandler<
        ListUserExpenseFinancialTransactionsQuery,
        Result<IReadOnlyList<FinancialTransactionDto>>
    >
{
    public async Task<Result<IReadOnlyList<FinancialTransactionDto>>> Handle(
        ListUserExpenseFinancialTransactionsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _service.GetUserExpenseTransactions(
            request.UserId,
            request.IncomeTransactionId,
            cancellationToken
        );
    }
}
