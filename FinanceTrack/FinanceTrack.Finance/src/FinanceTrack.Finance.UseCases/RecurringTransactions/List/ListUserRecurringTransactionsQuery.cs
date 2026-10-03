namespace FinanceTrack.Finance.UseCases.RecurringTransactions.List;

public sealed record ListUserRecurringTransactionsQuery(string UserId)
    : IQuery<IReadOnlyList<RecurringTransactionDto>>;
