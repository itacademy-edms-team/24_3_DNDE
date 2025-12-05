namespace FinanceTrack.Finance.UseCases.Transactions.Delete;

public sealed record DeleteIncomeTransactionCommand(Guid TransactionId, string UserId)
  : ICommand<Result>;
