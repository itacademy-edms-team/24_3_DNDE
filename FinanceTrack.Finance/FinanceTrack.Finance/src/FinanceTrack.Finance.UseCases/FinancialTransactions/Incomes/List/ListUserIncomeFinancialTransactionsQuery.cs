namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.List;

public sealed record ListUserIncomeFinancialTransactionsQuery(string UserId)
    : IQuery<IReadOnlyList<FinancialTransactionDto>>;
