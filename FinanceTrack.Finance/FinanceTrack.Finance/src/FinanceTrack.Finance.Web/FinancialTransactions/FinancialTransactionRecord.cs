namespace FinanceTrack.Finance.Web.Transactions;

public sealed record FinancialTransactionRecord(
    Guid Id,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    string Type
);
