using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Wallets;
using FinanceTrack.Finance.UseCases.Wallets.List;
using MR.AspNetCore.Pagination;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class UserWalletListQueryService(AppDbContext dbContext, IPaginationService service)
    : IUserWalletListQueryService
{
    public async Task<WalletPageDto> ListAsync(
        string userId,
        bool isArchived,
        string? afterCursor,
        int pageSize,
        CancellationToken cancel
    )
    {
        var query = dbContext
            .Wallets.AsNoTracking()
            .Where(w => w.UserId == userId && w.IsArchived == isArchived);

        if (
            afterCursor != null
            && WalletPaginationCursor.TryDecode(afterCursor, out var lastCreatedAt, out var lastId)
        )
        {
            query = query.Where(w =>
                w.CreatedAtUtc > lastCreatedAt || (w.CreatedAtUtc == lastCreatedAt && w.Id > lastId)
            );
        }

        var page = await service.KeysetPaginateAsync<Wallet, Wallet>(
            source: query,
            builderAction: b => b.Ascending(w => w.CreatedAtUtc).Ascending(w => w.Id),
            getReferenceAsync: _ => Task.FromResult<Wallet?>(null), // Намеренно сводим к null, т.к. курсор уже предоставляет все необходимые данные для пагинации
            map: q => q,
            queryModel: new KeysetQueryModel { Size = pageSize }
        );

        var last = page.Data.LastOrDefault();
        var nextCursor =
            page.HasNext && last != null
                ? WalletPaginationCursor.Encode(last.CreatedAtUtc, last.Id)
                : null;

        var dtos = page
            .Data.Select(w => new WalletDto(
                w.Id,
                w.Name,
                w.WalletType.Name,
                w.Balance,
                w.AllowNegativeBalance,
                w.TargetAmount,
                w.TargetDate,
                w.IsArchived
            ))
            .ToList();

        return new WalletPageDto(dtos, nextCursor, page.HasNext);
    }
}
