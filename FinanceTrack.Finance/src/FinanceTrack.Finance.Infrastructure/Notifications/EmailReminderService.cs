using System.Text;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.Infrastructure.Notifications;

public class EmailReminderService(
    ILogger<EmailReminderService> logger,
    IEmailSender emailSender,
    IRepository<RecurringTransaction> recurringRepository,
    IReadRepository<User> userRepository,
    IUnitOfWork unitOfWork
)
{
    public async Task SendRemindersAsync(DateOnly today, CancellationToken cancel = default)
    {
        var transactions = await recurringRepository.ListAsync(
            new RecurringTransactionsForReminderSpec(today),
            cancel
        );

        var pending = transactions
            .Select(rt => (Rule: rt, NextDueDate: ComputeNextDueDate(rt, today)))
            .Where(x => x.NextDueDate <= today.AddDays(7))
            .GroupBy(x => x.Rule.UserId)
            .ToList();

        if (pending.Count == 0)
            return;

        foreach (var group in pending)
        {
            var userId = Guid.Parse(group.Key);
            var user = await userRepository.FirstOrDefaultAsync(new UserByIdSpec(userId), cancel);

            if (user is null || !user.IsEmailNotificationsEnabled)
                continue;

            var items = group
                .OrderBy(x => x.NextDueDate)
                .Select(x => new ReminderItem(
                    x.Rule.Wallet.Name,
                    x.Rule.Name,
                    x.Rule.Amount,
                    x.NextDueDate
                ))
                .ToList();

            await emailSender.SendEmailAsync(
                user.Email,
                "Напоминание: предстоящие платежи",
                BuildEmailBody(items)
            );

            foreach (var item in group)
                item.Rule.MarkEmailReminderSent(today);

            logger.LogInformation(
                "Sent reminder to user {UserId} for {Count} upcoming transactions",
                userId,
                items.Count
            );
        }

        await unitOfWork.SaveChangesAsync(cancel);
    }

    private static DateOnly ComputeNextDueDate(RecurringTransaction rule, DateOnly today)
    {
        var daysInCurrentMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var thisMonthDue = new DateOnly(
            today.Year,
            today.Month,
            Math.Min(rule.DayOfMonth, daysInCurrentMonth)
        );

        if (thisMonthDue >= today)
            return thisMonthDue;

        var nextMonth = today.AddMonths(1);
        var daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        return new DateOnly(
            nextMonth.Year,
            nextMonth.Month,
            Math.Min(rule.DayOfMonth, daysInNextMonth)
        );
    }

    private static string BuildEmailBody(IReadOnlyList<ReminderItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Предстоящие платежи:");
        sb.AppendLine();
        foreach (var item in items)
            sb.AppendLine(
                $"• [{item.WalletName}] {item.Name} — {item.Amount:F2} ₽, {item.DueDate:dd.MM.yyyy}"
            );
        return sb.ToString();
    }

    private record ReminderItem(string WalletName, string Name, decimal Amount, DateOnly DueDate);
}
