using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.Web.BackgroundServices;

public class RecurringTransactionBackgroundService(
    IServiceScopeFactory _scopeFactory,
    ILogger<RecurringTransactionBackgroundService> _logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurringTransactionBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor =
                    scope.ServiceProvider.GetRequiredService<RecurringTransactionProcessorService>();

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var created = await processor.ProcessAsync(today, stoppingToken);

                if (created > 0)
                {
                    _logger.LogInformation(
                        "RecurringTransactionBackgroundService: created {Count} transactions for {Date}.",
                        created,
                        today
                    );
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Error in RecurringTransactionBackgroundService."
                );
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
