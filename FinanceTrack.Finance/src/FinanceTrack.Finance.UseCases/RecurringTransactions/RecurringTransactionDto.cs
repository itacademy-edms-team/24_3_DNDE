namespace FinanceTrack.Finance.UseCases.RecurringTransactions;

public sealed record RecurringTransactionDto(
    Guid Id,
    Guid WalletId,
    Guid? CategoryId,
    string Name,
    string? Description,
    string Type,
    decimal Amount,
    int DayOfMonth,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateOnly? LastProcessedDate,
    string? WalletName = null
);
