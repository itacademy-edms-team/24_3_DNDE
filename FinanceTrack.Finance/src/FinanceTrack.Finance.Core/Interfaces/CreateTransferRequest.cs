namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record CreateTransferRequest(
    string UserId,
    Guid FromWalletId,
    Guid ToWalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate
);
