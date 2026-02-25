namespace FinanceTrack.Finance.UseCases.Wallets.Get;

public sealed record GetWalletQuery(Guid WalletId, string UserId)
    : IQuery<Result<WalletDto>>;
