using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.Services;

public class RecurringTransactionProcessorService(
    IRepository<RecurringTransaction> _recurringRepo,
    IRepository<FinancialTransaction> _transactionRepo,
    IRepository<Wallet> _walletRepo,
    ILogger<RecurringTransactionProcessorService> _logger
)
{
    public async Task<int> ProcessAsync(DateOnly today, CancellationToken ct = default)
    {
        var spec = new ActiveRecurringTransactionsSpec();
        var activeRules = await _recurringRepo.ListAsync(spec, ct);

        var created = 0;

        foreach (var rule in activeRules)
        {
            try
            {
                created += await ProcessRule(rule, today, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing recurring transaction {RecurringId}",
                    rule.Id
                );
            }
        }

        return created;
    }

    private async Task<int> ProcessRule(
        RecurringTransaction rule,
        DateOnly today,
        CancellationToken ct
    )
    {
        var created = 0;

        // Determine the first month to process
        var startMonth = rule.LastProcessedDate.HasValue
            ? rule.LastProcessedDate.Value.AddMonths(1)
            : rule.StartDate;

        // Normalize to first day of month for comparison
        var currentMonth = new DateOnly(startMonth.Year, startMonth.Month, 1);
        var todayMonth = new DateOnly(today.Year, today.Month, 1);

        while (currentMonth <= todayMonth)
        {
            var operationDate = new DateOnly(
                currentMonth.Year,
                currentMonth.Month,
                Math.Min(rule.DayOfMonth, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month))
            );

            // Only create if the operation date has arrived and it's within the active range
            if (operationDate <= today && operationDate >= rule.StartDate)
            {
                if (rule.EndDate.HasValue && operationDate > rule.EndDate.Value)
                    break;

                // Check if already processed for this month
                if (rule.LastProcessedDate.HasValue && operationDate <= rule.LastProcessedDate.Value)
                {
                    currentMonth = currentMonth.AddMonths(1);
                    continue;
                }

                var wallet = await _walletRepo.GetByIdAsync(rule.WalletId, ct);
                if (wallet is null || wallet.IsArchived)
                {
                    _logger.LogWarning(
                        "Wallet {WalletId} not found or archived for recurring {RecurringId}. Skipping.",
                        rule.WalletId,
                        rule.Id
                    );
                    break;
                }

                FinancialTransaction transaction;

                if (rule.TransactionType == RecurringTransactionType.Income)
                {
                    transaction = FinancialTransaction.CreateIncome(
                        rule.UserId,
                        rule.WalletId,
                        rule.Name,
                        rule.Amount,
                        operationDate,
                        rule.CategoryId,
                        rule.Id
                    );
                    wallet.Credit(rule.Amount);
                }
                else
                {
                    // For expense, skip if insufficient funds and not allowed negative
                    if (!wallet.AllowNegativeBalance && wallet.Balance < rule.Amount)
                    {
                        _logger.LogWarning(
                            "Insufficient funds for recurring expense {RecurringId} on wallet {WalletId}. Skipping.",
                            rule.Id,
                            rule.WalletId
                        );
                        currentMonth = currentMonth.AddMonths(1);
                        continue;
                    }

                    transaction = FinancialTransaction.CreateExpense(
                        rule.UserId,
                        rule.WalletId,
                        rule.Name,
                        rule.Amount,
                        operationDate,
                        rule.CategoryId,
                        rule.Id
                    );
                    wallet.Debit(rule.Amount);
                }

                await _transactionRepo.AddAsync(transaction, ct);
                await _walletRepo.UpdateAsync(wallet, ct);

                rule.MarkProcessed(operationDate);
                await _recurringRepo.UpdateAsync(rule, ct);

                created++;

                _logger.LogInformation(
                    "Created {Type} transaction {TransactionId} from recurring {RecurringId} for date {Date}",
                    rule.TransactionType.Name,
                    transaction.Id,
                    rule.Id,
                    operationDate
                );
            }

            currentMonth = currentMonth.AddMonths(1);
        }

        return created;
    }
}
