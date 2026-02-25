namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Update;

public sealed record UpdateTransactionCommand(
    Guid TransactionId,
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId
) : ICommand<Result<FinancialTransactionDto>>;
