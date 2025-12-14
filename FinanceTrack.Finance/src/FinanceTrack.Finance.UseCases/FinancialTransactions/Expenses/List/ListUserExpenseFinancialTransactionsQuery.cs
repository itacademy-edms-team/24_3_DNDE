namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;

public sealed record ListUserExpenseFinancialTransactionsQuery(
    string UserId,
    Guid IncomeTransactionId
) : IQuery<Result<IReadOnlyList<FinancialTransactionDto>>>;

