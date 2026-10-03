namespace FinanceTrack.Finance.Core.Interfaces;

public record CreateExpenseRequest(
    string UserId,
    Guid WalletId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId = null,
    Guid? RecurringTransactionId = null
);
