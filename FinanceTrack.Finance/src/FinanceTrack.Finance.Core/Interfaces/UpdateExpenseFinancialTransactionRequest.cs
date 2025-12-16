namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record UpdateExpenseFinancialTransactionRequest(
    Guid TransactionId,
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    Guid IncomeTransactionId
);
