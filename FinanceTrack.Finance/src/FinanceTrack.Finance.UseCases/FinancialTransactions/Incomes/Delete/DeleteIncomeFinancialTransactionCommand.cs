namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;

public sealed record DeleteIncomeFinancialTransactionCommand(Guid TransactionId, string UserId)
    : ICommand<Result>;
