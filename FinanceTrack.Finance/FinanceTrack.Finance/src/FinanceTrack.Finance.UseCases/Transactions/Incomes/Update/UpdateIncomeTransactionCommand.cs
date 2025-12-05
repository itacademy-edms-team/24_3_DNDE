namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.Update;

public sealed record UpdateIncomeTransactionCommand(
  Guid TransactionId,
  string Name,
  string UserId,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly
) : ICommand<Result<TransactionDto>>;
