namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed class ListUserWalletsHandler(IUserWalletListQueryService walletListService)
    : IQueryHandler<ListUserWalletsQuery, Result<WalletPageDto>>
{
    public async Task<Result<WalletPageDto>> Handle(
        ListUserWalletsQuery request,
        CancellationToken ct
    )
    {
        var page = await walletListService.ListAsync(
            request.UserId,
            isArchived: false,
            request.AfterCursor,
            request.PageSize,
            ct
        );
        return Result.Success(page);
    }
}
