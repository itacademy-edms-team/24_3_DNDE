namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record CreateExpenseFinancialTransactionRequest(
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    Guid IncomeTransactionId
);
