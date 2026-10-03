namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;

public sealed record DeleteTransactionCommand(Guid TransactionId, string UserId)
    : ICommand<Result>;
