namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Create;

public sealed record CreateRecurringTransactionCommand(
    string UserId,
    Guid WalletId,
    Guid? CategoryId,
    string Name,
    string? Description,
    string Type, // "Income" or "Expense"
    decimal Amount,
    int DayOfMonth,
    DateOnly StartDate,
    DateOnly? EndDate
) : ICommand<Result<Guid>>;
