namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Transfer;

public sealed record CreateTransferCommand(
    string UserId,
    Guid FromWalletId,
    Guid ToWalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate
) : ICommand<Result<Guid>>;
