using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Transfer;

public sealed record CreateTransferCommand(
    string UserId,
    Guid FromWalletId,
    Guid ToWalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate
)
    : CreateTransferRequest(
        UserId,
        FromWalletId,
        ToWalletId,
        Name,
        Amount,
        OperationDate
    ),
        ICommand<Result<Guid>>;
