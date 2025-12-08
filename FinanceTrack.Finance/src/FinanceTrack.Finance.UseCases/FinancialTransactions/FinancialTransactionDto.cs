namespace FinanceTrack.Finance.UseCases.FinancialTransactions;

public sealed record FinancialTransactionDto(
    Guid Id,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    string Type
);
