using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Data.Cleanup;

/// <summary>
/// Фоновый сервис чистки сирот модуля Media: брошенные загрузки, файлы без ссылок, надгробия.
/// </summary>
internal sealed class MediaCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<MediaCleanupOptions> options,
    TimeProvider clock,
    ILogger<MediaCleanupService> logger)
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
                var cleaner = scope.ServiceProvider.GetRequiredService<MediaCleaner>();
                var now = clock.GetUtcNow().UtcDateTime;

                var abandoned = await cleaner.PurgeAbandonedAsync(
                    now - settings.AbandonedRetention, settings.BatchSize, stoppingToken);
                var unreferenced = await cleaner.PurgeUnreferencedAsync(
                    now - settings.UnreferencedRetention, settings.BatchSize, stoppingToken);
                var deleted = await cleaner.PurgeDeletedAsync(
                    now - settings.DeletedRetention, settings.BatchSize, stoppingToken);

                if (abandoned + unreferenced + deleted > 0)
                    logger.LogInformation(
                        "Media cleanup: {Abandoned} abandoned, {Unreferenced} unreferenced, {Deleted} tombstones",
                        abandoned, unreferenced, deleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Media cleanup failed");
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
