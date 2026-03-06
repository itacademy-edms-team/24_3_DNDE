namespace FinanceTrack.Finance.UseCases.RecurringTransactions.ToggleActive;

public sealed record ToggleRecurringTransactionCommand(
    Guid RecurringId,
    string UserId,
    bool IsActive
) : ICommand<Result>;
