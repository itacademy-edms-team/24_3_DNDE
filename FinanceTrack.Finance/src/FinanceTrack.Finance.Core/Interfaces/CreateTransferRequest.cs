namespace FinanceTrack.Finance.Core.Interfaces;

public record CreateTransferRequest(
    string UserId,
    Guid FromWalletId,
    Guid ToWalletId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate
);
