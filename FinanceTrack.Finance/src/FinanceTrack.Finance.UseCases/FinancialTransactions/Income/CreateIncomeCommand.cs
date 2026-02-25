namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Income;

public sealed record CreateIncomeCommand(
    string UserId,
    Guid WalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId
) : ICommand<Result<Guid>>;
