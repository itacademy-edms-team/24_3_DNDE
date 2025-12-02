using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.Web.Transactions;

public sealed record TransactionRecord(
  Guid Id,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly,
  string Type
);
