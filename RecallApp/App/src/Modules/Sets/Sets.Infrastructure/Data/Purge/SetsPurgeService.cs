using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Purge;

/// <summary>
/// Фоновый сервис чистки мягко удалённых наборов и элементов модуля Sets.
/// </summary>
internal sealed class SetsPurgeService(
    IServiceScopeFactory scopeFactory,
    IOptions<SetsPurgeOptions> options,
    TimeProvider clock,
    ILogger<SetsPurgeService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();

                var (sets, elements) = await SetsPurger.PurgeAsync(
                    context,
                    clock.GetUtcNow().UtcDateTime - settings.Retention,
                    settings.BatchSize,
                    stoppingToken);

                if (sets > 0 || elements > 0)
                    logger.LogInformation("Purged {Sets} sets and {Elements} elements past retention", sets, elements);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Soft-deleted content purge failed");
            }

            try
            {
                await Task.Delay(settings.Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
