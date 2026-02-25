namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expense;

public sealed record CreateExpenseCommand(
    string UserId,
    Guid WalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId
) : ICommand<Result<Guid>>;
