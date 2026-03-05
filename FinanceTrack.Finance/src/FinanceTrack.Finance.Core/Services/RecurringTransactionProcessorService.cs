using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.Core.Services;

public class RecurringTransactionProcessorService(
    IRepository<RecurringTransaction> recurringRepo,
    IRepository<FinancialTransaction> transactionRepo,
    IRepository<Wallet> walletRepo,
    ILogger<RecurringTransactionProcessorService> logger,
    IUnitOfWork unitOfWork
)
{
    public async Task<int> ProcessAsync(DateOnly today, CancellationToken ct = default)
    {
        var spec = new ActiveRecurringTransactionsSpec();
        var activeRules = await recurringRepo.ListAsync(spec, ct);

        var created = 0;

        foreach (var rule in activeRules)
        {
            try
            {
                created += await ProcessRule(rule, today, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
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
                Math.Min(
                    rule.DayOfMonth,
                    DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month)
                )
            );

            // Only create if the operation date has arrived and it's within the active range
            if (operationDate <= today && operationDate >= rule.StartDate)
            {
                if (rule.EndDate.HasValue && operationDate > rule.EndDate.Value)
                    break;

                // Check if already processed for this month
                if (
                    rule.LastProcessedDate.HasValue
                    && operationDate <= rule.LastProcessedDate.Value
                )
                {
                    currentMonth = currentMonth.AddMonths(1);
                    continue;
                }

                var spec = new WalletByIdSpec(rule.WalletId);
                var wallet = await walletRepo.FirstOrDefaultAsync(spec, ct);
                if (wallet is null || wallet.IsArchived)
                {
                    logger.LogWarning(
                        "Wallet {WalletId} not found or archived for recurring {RecurringId}. Skipping.",
                        rule.WalletId,
                        rule.Id
                    );
                    break;
                }

                // Create transaction and apply its effect to the wallet
                var transaction = CreateTransactionForRuleAndDate(rule, wallet, operationDate);

                // For expense, we may skip if insufficient funds and negative not allowed
                if (transaction is null)
                {
                    currentMonth = currentMonth.AddMonths(1);
                    continue;
                }

                await transactionRepo.AddAsync(transaction, ct);

                rule.MarkProcessed(operationDate);

                await unitOfWork.SaveChangesAsync(ct);

                created++;

                logger.LogInformation(
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

    /// <summary>
    /// Creates a financial transaction for the given recurring rule and date
    /// and applies its effect to the wallet. For expense rules, returns null
    /// when there are insufficient funds and negative balance is not allowed.
    /// </summary>
    private FinancialTransaction? CreateTransactionForRuleAndDate(
        RecurringTransaction rule,
        Wallet wallet,
        DateOnly operationDate
    )
    {
        if (rule.TransactionType == RecurringTransactionType.Income)
        {
            var transaction = FinancialTransaction.CreateIncome(
                rule.UserId,
                rule.WalletId,
                rule.Name,
                rule.Amount,
                operationDate,
                rule.CategoryId,
                rule.Id
            );
            wallet.Credit(rule.Amount);
            wallet.AddTransaction(transaction);
            return transaction;
        }

        // For expense, skip if insufficient funds and negative balance is not allowed
        if (!wallet.AllowNegativeBalance && wallet.Balance < rule.Amount)
        {
            logger.LogWarning(
                "Insufficient funds for recurring expense {RecurringId} on wallet {WalletId}. Skipping.",
                rule.Id,
                rule.WalletId
            );
            return null;
        }

        var expenseTransaction = FinancialTransaction.CreateExpense(
            rule.UserId,
            rule.WalletId,
            rule.Name,
            rule.Amount,
            operationDate,
            rule.CategoryId,
            rule.Id
        );
        wallet.Debit(rule.Amount);
        wallet.AddTransaction(expenseTransaction);
        return expenseTransaction;
    }
}
