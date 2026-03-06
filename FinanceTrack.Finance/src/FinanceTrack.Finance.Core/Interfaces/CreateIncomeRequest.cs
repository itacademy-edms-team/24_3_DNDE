namespace FinanceTrack.Finance.Core.Interfaces;

public record CreateIncomeRequest(
    string UserId,
    Guid WalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId = null,
    Guid? RecurringTransactionId = null
);
