using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class WalletAnalyticsQueryService(AppDbContext dbContext) : IWalletAnalyticsQueryService
{
    public async Task<Result<WalletOverviewDto>> GetWalletOverview(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var wallet = await dbContext.Wallets.FirstOrDefaultAsync(
            w => w.Id == walletId && w.UserId == userId,
            ct
        );

        if (wallet is null)
            return Result.NotFound("Wallet not found");

        var transactions = await dbContext
            .FinancialTransactions.Where(t =>
                t.WalletId == walletId
                && t.UserId == userId
                && t.OperationDate >= from
                && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var income = transactions
            .Where(t =>
                t.TransactionType == FinancialTransactionType.Income
                || t.TransactionType == FinancialTransactionType.TransferIn
            )
            .Sum(t => t.Amount);
        var expense = transactions
            .Where(t =>
                t.TransactionType == FinancialTransactionType.Expense
                || t.TransactionType == FinancialTransactionType.TransferOut
            )
            .Sum(t => t.Amount);

        return Result.Success(new WalletOverviewDto(
            wallet.Id,
            wallet.Name,
            wallet.Balance,
            income,
            expense,
            income - expense
        ));
    }

    public async Task<Result<CashFlowDto>> GetWalletCashFlow(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var wallet = await dbContext.Wallets.FirstOrDefaultAsync(
            w => w.Id == walletId && w.UserId == userId,
            ct
        );

        if (wallet is null)
            return Result.NotFound("Wallet not found");

        var transactions = await dbContext
            .FinancialTransactions.Where(t =>
                t.WalletId == walletId
                && t.UserId == userId
                && t.OperationDate >= from
                && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var periods = transactions
            .GroupBy(t => new { t.OperationDate.Year, t.OperationDate.Month })
            .Select(g =>
            {
                var periodIncome = g.Where(t =>
                        t.TransactionType == FinancialTransactionType.Income
                        || t.TransactionType == FinancialTransactionType.TransferIn
                    )
                    .Sum(t => t.Amount);
                var periodExpense = g.Where(t =>
                        t.TransactionType == FinancialTransactionType.Expense
                        || t.TransactionType == FinancialTransactionType.TransferOut
                    )
                    .Sum(t => t.Amount);
                return new CashFlowPeriodDto(
                    g.Key.Year,
                    g.Key.Month,
                    periodIncome,
                    periodExpense,
                    periodIncome - periodExpense
                );
            })
            .OrderBy(p => p.Year)
            .ThenBy(p => p.Month)
            .ToList();

        return Result.Success(new CashFlowDto(periods));
    }

    public async Task<Result<CategoriesAnalyticsDto>> GetWalletCategoriesAnalytics(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var walletExists = await dbContext.Wallets.AnyAsync(
            w => w.Id == walletId && w.UserId == userId,
            ct
        );

        if (!walletExists)
            return Result.NotFound("Wallet not found");

        var transactions = await dbContext
            .FinancialTransactions.Where(t =>
                t.WalletId == walletId
                && t.UserId == userId
                && t.OperationDate >= from
                && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var categories = await dbContext.Categories.Where(c => c.UserId == userId).ToListAsync(ct);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        var incomeByCategory = CategoryBreakdownHelper.Build(
            transactions, FinancialTransactionType.Income, categoryDict);
        var expenseByCategory = CategoryBreakdownHelper.Build(
            transactions, FinancialTransactionType.Expense, categoryDict);

        return Result.Success(new CategoriesAnalyticsDto(incomeByCategory, expenseByCategory));
    }
}
