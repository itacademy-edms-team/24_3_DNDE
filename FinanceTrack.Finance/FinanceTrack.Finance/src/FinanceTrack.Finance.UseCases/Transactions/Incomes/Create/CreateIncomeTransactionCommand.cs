namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.Create;

public sealed record CreateIncomeTransactionCommand(
  string UserId,
  string Name,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly
) : ICommand<Result<Guid>>;
