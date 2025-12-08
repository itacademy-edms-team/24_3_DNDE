namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Create;

public sealed record CreateIncomeFinancialTransactionCommand(
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly
) : ICommand<Result<Guid>>;
