namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed record ListUserArchiveWalletsQuery(string UserId, string? AfterCursor, int PageSize)
    : IQuery<Result<WalletPageDto>>;
