namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Update;

public sealed record UpdateIncomeFinancialTransactionCommand(
    Guid TransactionId,
    string Name,
    string UserId,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly
) : ICommand<Result<FinancialTransactionDto>>;
