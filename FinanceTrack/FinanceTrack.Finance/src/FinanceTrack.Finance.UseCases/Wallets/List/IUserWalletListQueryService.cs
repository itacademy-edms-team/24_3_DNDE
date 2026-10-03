namespace FinanceTrack.Finance.UseCases.Wallets.List;

public interface IUserWalletListQueryService
{
    Task<WalletPageDto> ListAsync(
        string userId,
        bool isArchived,
        string? afterCursor,
        int pageSize,
        CancellationToken cancel
    );
}
