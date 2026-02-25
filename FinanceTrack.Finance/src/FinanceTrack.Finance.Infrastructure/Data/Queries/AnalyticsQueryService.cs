using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Analytics;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class AnalyticsQueryService(AppDbContext _dbContext) : IAnalyticsQueryService
{
    public async Task<OverviewAnalyticsDto> GetOverview(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var wallets = await _dbContext
            .Wallets.Where(w => w.UserId == userId && !w.IsArchived)
            .ToListAsync(ct);

        var transactions = await _dbContext
            .FinancialTransactions.Where(t =>
                t.UserId == userId && t.OperationDate >= from && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var totalBalance = wallets.Sum(w => w.Balance);

        var accountSummaries = new List<AccountSummaryDto>();
        foreach (var wallet in wallets)
        {
            var walletTransactions = transactions.Where(t => t.WalletId == wallet.Id).ToList();
            var income = walletTransactions
                .Where(t => t.TransactionType == FinancialTransactionType.Income
                    || t.TransactionType == FinancialTransactionType.TransferIn)
                .Sum(t => t.Amount);
            var expense = walletTransactions
                .Where(t => t.TransactionType == FinancialTransactionType.Expense
                    || t.TransactionType == FinancialTransactionType.TransferOut)
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

    public async Task<AccountAnalyticsDto> GetAccountAnalytics(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var wallet = await _dbContext
            .Wallets.FirstOrDefaultAsync(
                w => w.Id == walletId && w.UserId == userId,
                ct
            );

        if (wallet is null)
            throw new InvalidOperationException("Wallet not found.");

        var transactions = await _dbContext
            .FinancialTransactions.Where(t =>
                t.WalletId == walletId
                && t.UserId == userId
                && t.OperationDate >= from
                && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var income = transactions
            .Where(t => t.TransactionType == FinancialTransactionType.Income
                || t.TransactionType == FinancialTransactionType.TransferIn)
            .Sum(t => t.Amount);
        var expense = transactions
            .Where(t => t.TransactionType == FinancialTransactionType.Expense
                || t.TransactionType == FinancialTransactionType.TransferOut)
            .Sum(t => t.Amount);

        // Category breakdowns
        var categories = await _dbContext
            .Categories.Where(c => c.UserId == userId)
            .ToListAsync(ct);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        var incomeByCategory = transactions
            .Where(t => t.TransactionType == FinancialTransactionType.Income)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var amount = g.Sum(t => t.Amount);
                var totalForType = income > 0 ? income : 1m;
                return new CategoryBreakdownDto(
                    g.Key,
                    g.Key.HasValue && categoryDict.TryGetValue(g.Key.Value, out var name)
                        ? name
                        : null,
                    amount,
                    Math.Round(amount / totalForType * 100, 2)
                );
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var expenseByCategory = transactions
            .Where(t => t.TransactionType == FinancialTransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var amount = g.Sum(t => t.Amount);
                var totalForType = expense > 0 ? expense : 1m;
                return new CategoryBreakdownDto(
                    g.Key,
                    g.Key.HasValue && categoryDict.TryGetValue(g.Key.Value, out var name)
                        ? name
                        : null,
                    amount,
                    Math.Round(amount / totalForType * 100, 2)
                );
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        return new AccountAnalyticsDto(
            wallet.Id,
            wallet.Name,
            wallet.Balance,
            income,
            expense,
            incomeByCategory,
            expenseByCategory
        );
    }

    public async Task<CashFlowDto> GetCashFlow(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var transactions = await _dbContext
            .FinancialTransactions.Where(t =>
                t.UserId == userId && t.OperationDate >= from && t.OperationDate <= to
            )
            .ToListAsync(ct);

        var periods = transactions
            .GroupBy(t => new { t.OperationDate.Year, t.OperationDate.Month })
            .Select(g =>
            {
                var periodIncome = g
                    .Where(t => t.TransactionType == FinancialTransactionType.Income)
                    .Sum(t => t.Amount);
                var periodExpense = g
                    .Where(t => t.TransactionType == FinancialTransactionType.Expense)
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
        var savingsWallets = await _dbContext
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
                w.TargetAmount.Value > 0
                    ? Math.Round(w.Balance / w.TargetAmount.Value * 100, 2)
                    : 0
            ))
            .ToList();

        return new SavingsProgressDto(accounts);
    }
}
