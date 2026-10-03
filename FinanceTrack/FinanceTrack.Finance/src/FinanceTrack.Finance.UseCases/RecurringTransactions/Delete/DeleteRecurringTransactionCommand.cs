namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Delete;

public sealed record DeleteRecurringTransactionCommand(Guid RecurringId, string UserId)
    : ICommand<Result>;
