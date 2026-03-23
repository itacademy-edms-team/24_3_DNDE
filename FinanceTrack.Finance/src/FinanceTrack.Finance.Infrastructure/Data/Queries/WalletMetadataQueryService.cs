using Ardalis.Result;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class WalletMetadataQueryService(AppDbContext dbContext) : IWalletMetadataQueryService
{
    public async Task<Result<YearMinMaxDto>> GetDateMinMax(
        string userId,
        CancellationToken ct = default
    )
    {
        var dates = await dbContext
            .FinancialTransactions.Where(t => t.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(t => t.OperationDate),
                Max = g.Max(t => t.OperationDate),
            })
            .FirstOrDefaultAsync(ct);

        if (dates is null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return Result.Success(new YearMinMaxDto(today, today));
        }

        return Result.Success(new YearMinMaxDto(dates.Min, dates.Max));
    }

    public async Task<Result<YearMinMaxDto>> GetDateMinMax(
        string userId,
        Guid walletId,
        CancellationToken ct = default
    )
    {
        var walletExists = await dbContext.Wallets.AnyAsync(
            w => w.Id == walletId && w.UserId == userId,
            ct
        );

        if (!walletExists)
            return Result.NotFound("Wallet not found");

        var dates = await dbContext
            .FinancialTransactions.Where(t => t.UserId == userId && t.WalletId == walletId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(t => t.OperationDate),
                Max = g.Max(t => t.OperationDate),
            })
            .FirstOrDefaultAsync(ct);

        if (dates is null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return Result.Success(new YearMinMaxDto(today, today));
        }

        return Result.Success(new YearMinMaxDto(dates.Min, dates.Max));
    }
}
