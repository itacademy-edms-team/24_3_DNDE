namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.List;

public sealed record ListUserIncomeTransactionsQuery(string UserId)
  : IQuery<IReadOnlyList<TransactionDTO>>;
