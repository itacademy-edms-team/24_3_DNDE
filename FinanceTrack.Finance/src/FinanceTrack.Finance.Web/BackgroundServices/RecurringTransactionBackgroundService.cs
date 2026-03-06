using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.Web.BackgroundServices;

public class RecurringTransactionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringTransactionBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RecurringTransactionBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor =
                    scope.ServiceProvider.GetRequiredService<RecurringTransactionProcessorService>();

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var created = await processor.ProcessAsync(today, stoppingToken);

                if (created > 0)
                {
                    logger.LogInformation(
                        "RecurringTransactionBackgroundService: created {Count} transactions for {Date}.",
                        created,
                        today
                    );
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in RecurringTransactionBackgroundService.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
