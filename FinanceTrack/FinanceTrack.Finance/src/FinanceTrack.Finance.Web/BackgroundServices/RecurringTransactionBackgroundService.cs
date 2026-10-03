using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Infrastructure.Notifications;

namespace FinanceTrack.Finance.Web.BackgroundServices;

public class RecurringTransactionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringTransactionBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancel)
    {
        logger.LogInformation("RecurringTransactionBackgroundService started.");

        while (!cancel.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor =
                    scope.ServiceProvider.GetRequiredService<RecurringTransactionProcessorService>();

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var created = await processor.ProcessAsync(today, cancel);

                if (created > 0)
                {
                    logger.LogInformation(
                        "RecurringTransactionBackgroundService: created {Count} transactions for {Date}.",
                        created,
                        today
                    );
                }

                var reminderService =
                    scope.ServiceProvider.GetRequiredService<RecurringTransactionsReminderService>();
                await reminderService.SendRemindersAsync(today, cancel);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in RecurringTransactionBackgroundService.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), cancel);
        }
    }
}
