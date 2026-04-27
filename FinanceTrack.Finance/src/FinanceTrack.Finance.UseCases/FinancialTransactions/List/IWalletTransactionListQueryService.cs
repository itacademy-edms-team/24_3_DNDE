namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public interface IWalletTransactionListQueryService
{
    Task<FinancialTransactionPageDto> ListAsync(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        string? afterCursor, // курсор клиента для пагинации. Если null - первая страница.
        int pageSize,
        CancellationToken cancel
    );
}
