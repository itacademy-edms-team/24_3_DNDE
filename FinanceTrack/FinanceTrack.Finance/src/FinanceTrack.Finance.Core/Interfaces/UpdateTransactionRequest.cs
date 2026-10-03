namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record UpdateTransactionRequest(
    Guid TransactionId,
    string UserId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId
);
