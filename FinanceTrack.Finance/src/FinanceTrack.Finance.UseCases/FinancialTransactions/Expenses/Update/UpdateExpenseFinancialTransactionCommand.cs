namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Update;

public sealed record UpdateExpenseFinancialTransactionCommand(
    Guid TransactionId,
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    Guid IncomeTransactionId
) : ICommand<Result<FinancialTransactionDto>>;
