using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class GeneralAnalyticsQueryService(AppDbContext dbContext) : IGeneralAnalyticsQueryService
{
    public async Task<OverviewAnalyticsDto> GetOverview(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    )
    {
        var wallets = await dbContext
            .Wallets.Where(w => w.UserId == userId && !w.IsArchived)
            .ToListAsync(cancel);

        var transactions = await dbContext
            .FinancialTransactions.Where(t =>
                t.UserId == userId && t.OperationDate >= from && t.OperationDate <= to
            )
            .ToListAsync(cancel);

        var totalBalance = wallets.Sum(w => w.Balance);

        var accountSummaries = new List<AccountSummaryDto>();
        foreach (var wallet in wallets)
        {
            var walletTransactions = transactions.Where(t => t.WalletId == wallet.Id).ToList();
            var income = walletTransactions
                .Where(t => t.TransactionType == FinancialTransactionType.Income)
                .Sum(t => t.Amount);
            var expense = walletTransactions
                .Where(t => t.TransactionType == FinancialTransactionType.Expense)
                .Sum(t => t.Amount);

            accountSummaries.Add(
                new AccountSummaryDto(
                    wallet.Id,
                    wallet.Name,
                    wallet.WalletType.Name,
                    wallet.Balance,
                    income,
                    expense
                )
            );
        }

        var totalIncome = transactions
            .Where(t => t.TransactionType == FinancialTransactionType.Income)
            .Sum(t => t.Amount);
        var totalExpense = transactions
            .Where(t => t.TransactionType == FinancialTransactionType.Expense)
            .Sum(t => t.Amount);

        return new OverviewAnalyticsDto(
            totalBalance,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            accountSummaries
        );
    }

    public async Task<CashFlowDto> GetCashFlow(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var transactions = await dbContext
            .FinancialTransactions.Where(t =>
                t.UserId == userId && t.OperationDate >= from && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var periods = transactions
            .GroupBy(t => new { t.OperationDate.Year, t.OperationDate.Month })
            .Select(g =>
            {
                var periodIncome = g.Where(t =>
                        t.TransactionType == FinancialTransactionType.Income
                    )
                    .Sum(t => t.Amount);
                var periodExpense = g.Where(t =>
                        t.TransactionType == FinancialTransactionType.Expense
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

        return new CashFlowDto(periods);
    }

    public async Task<SavingsProgressDto> GetSavingsProgress(
        string userId,
        CancellationToken ct = default
    )
    {
        var savingsWallets = await dbContext
            .Wallets.Where(w =>
                w.UserId == userId
                && w.WalletType == WalletType.Savings
                && !w.IsArchived
                && w.TargetAmount != null
            )
            .ToListAsync(ct);

        var accounts = savingsWallets
            .Select(w => new SavingsAccountProgressDto(
                w.Id,
                w.Name,
                w.Balance,
                w.TargetAmount!.Value,
                w.TargetDate,
                w.TargetAmount.Value > 0 ? Math.Round(w.Balance / w.TargetAmount.Value * 100, 2) : 0
            ))
            .ToList();

        return new SavingsProgressDto(accounts);
    }

    public async Task<CategoriesAnalyticsDto> GetCategoriesAnalytics(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var transactions = await dbContext
            .FinancialTransactions.Where(t =>
                t.UserId == userId && t.OperationDate >= from && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var categories = await dbContext.Categories.Where(c => c.UserId == userId).ToListAsync(ct);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        var incomeByCategory = CategoryBreakdownHelper.Build(
            transactions,
            FinancialTransactionType.Income,
            categoryDict
        );
        var expenseByCategory = CategoryBreakdownHelper.Build(
            transactions,
            FinancialTransactionType.Expense,
            categoryDict
        );

        return new CategoriesAnalyticsDto(incomeByCategory, expenseByCategory);
    }
}
