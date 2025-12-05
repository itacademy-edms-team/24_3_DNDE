namespace FinanceTrack.Finance.UseCases.Transactions;

public sealed record TransactionDto(
  Guid Id,
  string Name,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly,
  string Type
);
