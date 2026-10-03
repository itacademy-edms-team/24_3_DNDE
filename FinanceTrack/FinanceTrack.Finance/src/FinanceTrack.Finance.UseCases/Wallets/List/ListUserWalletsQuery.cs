namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed record ListUserWalletsQuery(string UserId, string? AfterCursor, int PageSize)
    : IQuery<Result<WalletPageDto>>;
