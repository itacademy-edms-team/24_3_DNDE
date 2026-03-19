namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Update;

public sealed record UpdateRecurringTransactionCommand(
    Guid RecurringId,
    string UserId,
    string Name,
    string? Description,
    decimal Amount,
    int DayOfMonth,
    DateOnly? EndDate,
    Guid? CategoryId
) : ICommand<Result<RecurringTransactionDto>>;
