namespace FinanceTrack.Finance.UseCases.RecurringTransactions;

public sealed record RecurringTransactionDto(
    Guid Id,
    Guid WalletId,
    Guid? CategoryId,
    string Name,
    string Type,
    decimal Amount,
    int DayOfMonth,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateOnly? LastProcessedDate
);
