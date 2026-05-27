using System.Text;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.Infrastructure.Notifications;

// TODO: раздельная отправка по каналам - что нужно изменить:
// 1. Добавить LastTelegramReminderDate в RecurringTransaction + миграция БД.
// 2. Создать RecurringTransactionsForTelegramReminderSpec (фильтр по своему флагу).
// 3. Разделить этот класс на EmailReminderService и TelegramReminderService.
// 4. BackgroundService вызывает оба сервиса независимо.
// Это позволит повторно отправить только по одному каналу, если первый упал,
// а также правильно обрабатывать пользователей, подключивших только один канал.

public class RecurringTransactionsReminderService(
    ILogger<RecurringTransactionsReminderService> logger,
    IEmailSender emailSender,
    ITelegramSender telegramSender,
    IRepository<RecurringTransaction> recurringRepository,
    IReadRepository<User> userRepository,
    IUnitOfWork unitOfWork
)
{
    public async Task SendRemindersAsync(DateOnly today, CancellationToken cancel = default)
    {
        // Одна выборка для двух внешних каналов.
        // При раздельной отправке потребуются изменения в Core.
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

            if (user is null)
                continue;

            // При раздельной отправке здесь не будет общего continue - каждый канал
            // проверяется независимо и помечает свой флаг отдельно.
            var hasAnyChannel =
                user.IsEmailNotificationsEnabled
                || (user.IsTelegramNotificationsEnabled && user.TelegramChatId.HasValue);

            if (!hasAnyChannel)
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

            if (user.IsEmailNotificationsEnabled)
            {
                await emailSender.SendEmailAsync(
                    user.Email,
                    "Напоминание: предстоящие платежи",
                    BuildEmailBody(items)
                );
            }

            if (user.IsTelegramNotificationsEnabled && user.TelegramChatId.HasValue)
            {
                await telegramSender.SendMessageAsync(
                    user.TelegramChatId.Value,
                    BuildTelegramMessage(items),
                    cancel
                );
            }

            // Единый флаг для обоих каналов. При раздельной отправке будет
            // MarkEmailReminderSent / MarkTelegramReminderSent - вызываются независимо.
            foreach (var item in group)
                item.Rule.MarkEmailReminderSent(today);

            logger.LogInformation(
                "Sent reminder to user {UserId} for {Count} upcoming transactions (email={Email}, telegram={Telegram})",
                userId,
                items.Count,
                user.IsEmailNotificationsEnabled,
                user.IsTelegramNotificationsEnabled && user.TelegramChatId.HasValue
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

        foreach (var walletGroup in items.GroupBy(i => i.WalletName))
        {
            sb.AppendLine();
            sb.AppendLine($"{walletGroup.Key}:");
            foreach (var item in walletGroup.OrderBy(i => i.DueDate))
                sb.AppendLine($"• {item.Name} — {item.Amount:F2} ₽, {item.DueDate:dd.MM.yyyy}");
        }

        return sb.ToString();
    }

    private static string BuildTelegramMessage(IReadOnlyList<ReminderItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🔔 Предстоящие платежи:");

        foreach (var walletGroup in items.GroupBy(i => i.WalletName))
        {
            sb.AppendLine();
            sb.AppendLine($"{walletGroup.Key}:");
            foreach (var item in walletGroup.OrderBy(i => i.DueDate))
                sb.AppendLine($"  • {item.Name} — {item.Amount:F2} ₽  ({item.DueDate:dd.MM.yyyy})");
        }

        return sb.ToString();
    }

    private record ReminderItem(string WalletName, string Name, decimal Amount, DateOnly DueDate);
}
