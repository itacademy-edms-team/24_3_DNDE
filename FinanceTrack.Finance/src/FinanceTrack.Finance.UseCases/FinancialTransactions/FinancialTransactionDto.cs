namespace FinanceTrack.Finance.UseCases.FinancialTransactions;

public sealed record FinancialTransactionDto(
    Guid Id,
    Guid WalletId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate,
    string Type,
    Guid? CategoryId,
    Guid? RelatedTransactionId,
    Guid? RecurringTransactionId,
    Guid? RelatedWalletId = null,
    string? RelatedWalletName = null,
    string? WalletName = null
);
