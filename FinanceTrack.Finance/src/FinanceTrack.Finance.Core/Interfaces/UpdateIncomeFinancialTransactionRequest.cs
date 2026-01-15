namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record UpdateIncomeFinancialTransactionRequest(
    Guid TransactionId,
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly
);
