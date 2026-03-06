namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed record ListUserWalletsQuery(string UserId)
    : IQuery<IReadOnlyList<WalletDto>>;
