namespace FinanceTrack.Finance.Web.Transactions;

public sealed record FinancialTransactionRecord(
    Guid Id,
    Guid WalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    string Type,
    Guid? CategoryId,
    Guid? RelatedTransactionId,
    Guid? RecurringTransactionId
);
