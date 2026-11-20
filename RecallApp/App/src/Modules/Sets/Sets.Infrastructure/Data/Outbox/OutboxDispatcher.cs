using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Доставляет готовые outbox-сообщения одной полосы по расписанию.
/// </summary>
/// <remarks>
/// <para>Экземпляр создаётся на каждую полосу (<c>fast</c> и <c>slow</c>).</para>
/// <para>В тестах воркер выключается флагом <see cref="OutboxOptions.Enabled"/>: прогон
/// вызывает обработку явно, без гонки с фоновым циклом.</para>
/// <para>Ошибка прохода логируется и не останавливает цикл.</para>
/// </remarks>
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
                await Task.Delay(settings.Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
