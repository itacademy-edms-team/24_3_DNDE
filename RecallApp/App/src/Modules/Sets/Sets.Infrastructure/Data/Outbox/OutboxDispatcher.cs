using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Фоновый воркер одной полосы outbox: периодически доставляет готовые сообщения своей полосы.
/// Регистрируется по экземпляру на полосу (fast и slow), чтобы медленная полоса не блокировала быструю.
/// </summary>
internal sealed class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    string lane,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcher> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        // В тестах воркер выключен: прогон вызывает обработку явно, без гонки с фоновым циклом.
        if (!settings.Enabled)
            return;

        using var timer = new PeriodicTimer(settings.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                var delivered = await processor.ProcessBatchAsync(
                    lane, settings.BatchSize, settings.MaxAttempts, stoppingToken);

                if (delivered > 0)
                    logger.LogInformation("Outbox lane {Lane} delivered {Count} messages", lane, delivered);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox lane {Lane} dispatch failed", lane);
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
