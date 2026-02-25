namespace FinanceTrack.Finance.UseCases.FinancialTransactions;

public sealed record FinancialTransactionDto(
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
