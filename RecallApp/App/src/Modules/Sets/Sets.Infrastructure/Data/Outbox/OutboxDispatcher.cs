using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Фоновый диспетчер outbox: периодически доставляет сообщения зарегистрированным обработчикам.
/// </summary>
internal sealed class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcher> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        // В тестах диспетчер выключен: прогон вызывает обработку явно, без гонки с фоновым циклом.
        if (!settings.Enabled)
            return;

        using var timer = new PeriodicTimer(settings.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                var delivered = await processor.ProcessBatchAsync(settings.BatchSize, stoppingToken);

                if (delivered > 0)
                    logger.LogInformation("Outbox delivered {Count} messages", delivered);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox dispatch failed");
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
