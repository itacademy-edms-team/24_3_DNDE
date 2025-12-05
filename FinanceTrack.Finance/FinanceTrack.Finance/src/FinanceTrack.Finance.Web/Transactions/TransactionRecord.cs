namespace FinanceTrack.Finance.Web.Transactions;

public sealed record TransactionRecord(
  Guid Id,
  string Name,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly,
  string Type
);
