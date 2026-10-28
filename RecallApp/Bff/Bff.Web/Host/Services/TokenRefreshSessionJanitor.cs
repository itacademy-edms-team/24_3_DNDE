using Host.Configuration;
using Microsoft.Extensions.Options;

namespace Host.Services;

/// <summary>
/// Фоновый уборщик записей замков обновления токенов.
/// </summary>
public sealed class TokenRefreshSessionJanitor(
    TokenRefreshSemaphores semaphores,
    IOptions<OidcOptions> oidcOptions,
    TimeProvider timeProvider,
    ILogger<TokenRefreshSessionJanitor> logger
) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _idleThreshold = oidcOptions.Value.TokenRefresh.SessionLockIdle;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval, timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    Sweep();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Token refresh session janitor sweep failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // штатная остановка приложения
        }
    }

    private void Sweep()
    {
        var now = timeProvider.GetUtcNow();
        var removed = 0;

        // Идём по семафорам: ловим записи без метки времени
        // (сессия вышла из строя до первого успешного refresh, значит это осиротевший семафор).
        foreach (var key in semaphores.Semaphores.Keys)
        {
            var isFresh =
                semaphores.LastRefreshedAt.TryGetValue(key, out var lastRefreshed)
                && now - lastRefreshed <= _idleThreshold;
            if (isFresh)
                continue;

            if (semaphores.TryEvict(key))
                removed++;
        }

        if (removed > 0)
            logger.LogDebug("Session janitor evicted {Count} idle refresh locks", removed);
    }
}
