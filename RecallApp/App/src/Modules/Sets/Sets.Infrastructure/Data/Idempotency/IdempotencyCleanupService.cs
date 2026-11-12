using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Фоновый сервис чистки журнала идемпотентности модуля Sets (ADR 0036: purge - задача владельца данных).
/// </summary>
internal sealed class IdempotencyCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<IdempotencyCleanupOptions> options,
    TimeProvider clock,
    ILogger<IdempotencyCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();

                var deleted = await IdempotencyCleaner.DeleteExpiredAsync(
                    context,
                    clock.GetUtcNow().UtcDateTime,
                    settings.BatchSize,
                    cancellationToken);

                if (deleted > 0)
                    logger.LogInformation("Removed {Count} expired idempotency keys", deleted);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Idempotency keys cleanup failed");
            }

            try
            {
                await Task.Delay(settings.Interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
