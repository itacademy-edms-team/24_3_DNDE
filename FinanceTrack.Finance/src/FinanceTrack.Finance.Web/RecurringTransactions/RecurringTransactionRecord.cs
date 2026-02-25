namespace FinanceTrack.Finance.Web.RecurringTransactions;

public sealed record RecurringTransactionRecord(
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
