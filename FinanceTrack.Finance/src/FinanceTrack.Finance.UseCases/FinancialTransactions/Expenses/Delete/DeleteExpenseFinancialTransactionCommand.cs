namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Delete;

public sealed record DeleteExpenseFinancialTransactionCommand(Guid TransactionId, string UserId)
    : ICommand<Result>;
