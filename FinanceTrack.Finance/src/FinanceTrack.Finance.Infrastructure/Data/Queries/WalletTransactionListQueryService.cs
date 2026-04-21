using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.FinancialTransactions.List;
using MR.AspNetCore.Pagination;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class WalletTransactionListQueryService(AppDbContext dbContext, IPaginationService service)
    : IWalletTransactionListQueryService
{
    public async Task<FinancialTransactionPageDto> ListAsync(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        string? afterCursor,
        int pageSize,
        CancellationToken cancel
    )
    {
        var query = dbContext
            .FinancialTransactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.WalletId == walletId)
            .Where(t => t.OperationDate >= from && t.OperationDate <= to);

        if (
            afterCursor != null
            && PaginationCursor.TryDecode(
                afterCursor,
                out var lastDate,
                out var lastCreatedAt,
                out var lastId
            )
        )
        {
            query = query.Where(t =>
                t.OperationDate < lastDate
                || (t.OperationDate == lastDate && t.CreatedAtUtc < lastCreatedAt)
                || (t.OperationDate == lastDate && t.CreatedAtUtc == lastCreatedAt && t.Id < lastId)
            );
        }

        var page = await service.KeysetPaginateAsync<FinancialTransaction, FinancialTransaction>(
            source: query,
            builderAction: b =>
                b.Descending(t => t.OperationDate)
                    .Descending(t => t.CreatedAtUtc)
                    .Descending(t => t.Id),
            getReferenceAsync: _ => Task.FromResult<FinancialTransaction?>(null), // reference заведомо сводим к null, т.к. передаваемый курсор уже даёт все необходимые данные для построения страницы
            map: q => q,
            queryModel: new KeysetQueryModel { Size = pageSize }
        );

        var last = page.Data.LastOrDefault();
        var nextCursor =
            page.HasNext && last != null
                ? PaginationCursor.Encode(last.OperationDate, last.CreatedAtUtc, last.Id)
                : null;

        var transactions = await EnrichAsync(page.Data, cancel);
        return new FinancialTransactionPageDto(transactions, nextCursor, page.HasNext);
    }

    private async Task<IReadOnlyList<FinancialTransactionDto>> EnrichAsync(
        IReadOnlyList<FinancialTransaction> transactions,
        CancellationToken cancel
    )
    {
        var relatedIds = transactions
            .Where(t => t.RelatedTransactionId.HasValue)
            .Select(t => t.RelatedTransactionId!.Value)
            .Distinct()
            .ToList();

        var relatedTransactions = relatedIds.Any()
            ? await dbContext
                .FinancialTransactions.AsNoTracking()
                .Where(t => relatedIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, cancel)
            : [];

        var relatedWalletIds = relatedTransactions.Any()
            ? relatedTransactions.Values.Select(t => t.WalletId).Distinct().ToList()
            : [];

        var relatedWallets = relatedWalletIds.Any()
            ? await dbContext
                .Wallets.AsNoTracking()
                .Where(w => relatedWalletIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, cancel)
            : [];

        return transactions
            .Select(t => FinancialTransactionDto.FromEntity(t, relatedTransactions, relatedWallets))
            .ToList();
    }
}
