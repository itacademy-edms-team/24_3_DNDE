namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed class ListUserArchiveWalletsHandler(IUserWalletListQueryService walletListService)
    : IQueryHandler<ListUserArchiveWalletsQuery, Result<WalletPageDto>>
{
    public async Task<Result<WalletPageDto>> Handle(
        ListUserArchiveWalletsQuery request,
        CancellationToken ct
    )
    {
        var page = await walletListService.ListAsync(
            request.UserId,
            isArchived: true,
            request.AfterCursor,
            request.PageSize,
            ct
        );
        return Result.Success(page);
    }
}
