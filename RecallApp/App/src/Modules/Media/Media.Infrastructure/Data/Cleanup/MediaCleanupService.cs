using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Data.Cleanup;

/// <summary>
/// Запускает фоновую чистку осиротевших данных модуля Media.
/// </summary>
/// <remarks>
/// Проходы выполняются раз в <see cref="MediaCleanupOptions.Interval"/>; ошибка прохода
/// логируется и не останавливает цикл.
/// </remarks>
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

        if (settings.PurgeOnStartup)
            await PurgeAbandonedOnStartupAsync(settings, stoppingToken);

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

    /// <summary>
    /// Dev-режим: перед первым циклом убирает все брошенные загрузки независимо от возраста.
    /// Иначе после серии неудачных загрузок лимит остаётся занятым до истечения ретенции.
    /// </summary>
    private async Task PurgeAbandonedOnStartupAsync(MediaCleanupOptions settings, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var cleaner = scope.ServiceProvider.GetRequiredService<MediaCleaner>();
            var now = clock.GetUtcNow().UtcDateTime;

            var total = 0;
            int removed;
            do
            {
                removed = await cleaner.PurgeAbandonedAsync(now, settings.BatchSize, stoppingToken);
                total += removed;
            }
            while (removed == settings.BatchSize && !stoppingToken.IsCancellationRequested);

            if (total > 0)
                logger.LogInformation("Media cleanup on startup: {Count} abandoned uploads removed", total);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Media cleanup on startup failed");
        }
    }
}
