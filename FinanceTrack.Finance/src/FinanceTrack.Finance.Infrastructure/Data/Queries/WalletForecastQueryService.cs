using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class WalletForecastQueryService(AppDbContext appDbContext) : IWalletForecastQueryService
{
    public async Task<Result<WalletForecastBalanceDto>> GetBalanceForecast(
        string userId,
        Guid walletId,
        CancellationToken ct = default
    )
    {
        var wallet = await appDbContext.Wallets.FirstOrDefaultAsync(
            w => w.Id == walletId && w.UserId == userId,
            ct
        );

        if (wallet is null)
            return Result.NotFound("Wallet not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endOfMonth = new DateOnly(
            today.Year,
            today.Month,
            DateTime.DaysInMonth(today.Year, today.Month)
        );

        // Wallet.Balance includes future existing transactions excluding recurring
        // Today's balance is Wallet.Balance - future transactions
        // Making sure not to forget about Recurring transactions
        var futureTransactions = await appDbContext
            .FinancialTransactions.Where(t =>
                t.WalletId == walletId && t.UserId == userId && t.OperationDate > today
            )
            .ToListAsync(ct);

        var futureIncome = futureTransactions
            .Where(t =>
                t.TransactionType == FinancialTransactionType.Income
                || t.TransactionType == FinancialTransactionType.TransferIn
            )
            .Sum(t => t.Amount);

        var futureExpense = futureTransactions
            .Where(t =>
                t.TransactionType == FinancialTransactionType.Expense
                || t.TransactionType == FinancialTransactionType.TransferOut
            )
            .Sum(t => t.Amount);

        var balanceToday = wallet.Balance - futureIncome + futureExpense;

        // Wallet.Balance includes future existing transactions excluding recurring
        // Balance at the end of the current month is Wallet.Balance - future transactions starting next month
        // Making sure not to forget about Recurring transactions
        var restOfMonthIncome = futureTransactions
            .Where(t =>
                t.OperationDate <= endOfMonth
                && (
                    t.TransactionType == FinancialTransactionType.Income
                    || t.TransactionType == FinancialTransactionType.TransferIn
                )
            )
            .Sum(t => t.Amount);

        var restOfMonthExpense = futureTransactions
            .Where(t =>
                t.OperationDate <= endOfMonth
                && (
                    t.TransactionType == FinancialTransactionType.Expense
                    || t.TransactionType == FinancialTransactionType.TransferOut
                )
            )
            .Sum(t => t.Amount);

        var balanceEndOfMonth = balanceToday + restOfMonthIncome - restOfMonthExpense;

        // Simulate recurring transactions that haven't fired yet this month
        var recurringRules = await appDbContext
            .RecurringTransactions.Where(r =>
                r.WalletId == walletId && r.UserId == userId && r.IsActive
            )
            .ToListAsync(ct);

        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);

        foreach (var rule in recurringRules)
        {
            var operationDay = Math.Min(
                rule.DayOfMonth,
                DateTime.DaysInMonth(today.Year, today.Month)
            );
            var operationDate = new DateOnly(today.Year, today.Month, operationDay);

            if (operationDate < rule.StartDate)
                continue;
            if (rule.EndDate.HasValue && operationDate > rule.EndDate.Value)
                continue;

            var alreadyProcessed =
                rule.LastProcessedDate.HasValue
                && new DateOnly(
                    rule.LastProcessedDate.Value.Year,
                    rule.LastProcessedDate.Value.Month,
                    1
                ) >= currentMonthStart;

            if (alreadyProcessed)
                continue;

            if (rule.TransactionType == RecurringTransactionType.Income)
                balanceEndOfMonth += rule.Amount;
            else
                balanceEndOfMonth -= rule.Amount;
        }

        return Result.Success(new WalletForecastBalanceDto(balanceToday, balanceEndOfMonth));
    }
}
