using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public sealed class ListWalletTransactionsHandler(
    IWalletTransactionListQueryService transactionListService,
    IReadRepository<Wallet> walletRepo
) : IQueryHandler<ListWalletTransactionsQuery, Result<FinancialTransactionPageDto>>
{
    public async Task<Result<FinancialTransactionPageDto>> Handle(
        ListWalletTransactionsQuery request,
        CancellationToken cancel
    )
    {
        var walletSpec = new WalletByIdSpec(request.WalletId);
        var wallet = await walletRepo.FirstOrDefaultAsync(walletSpec, cancel);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        var transactionsResult = await transactionListService.ListAsync(
            request.UserId,
            request.WalletId,
            request.From,
            request.To,
            request.AfterCursor,
            request.PageSize,
            cancel
        );

        return transactionsResult;
    }
}
