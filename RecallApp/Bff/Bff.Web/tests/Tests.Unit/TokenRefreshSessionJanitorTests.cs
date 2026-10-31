using Host.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary>
/// Фоновую уборку записей замков запускает по таймеру:
/// удаляются простаивающие и осиротевшие записи, свежие и занятые сохраняются.
/// </summary>
public class TokenRefreshSessionJanitorTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);

    private readonly FakeTimeProvider _time = new(Start);
    private readonly TokenRefreshSemaphores _semaphores = new();

    private TokenRefreshSessionJanitor CreateJanitor() =>
        new(_semaphores, TestOidcOptions.Wrap(), _time, NullLogger<TokenRefreshSessionJanitor>.Instance);

    /// <summary>Уборщик работает в фоне: ждём, пока проход по таймеру применит результат.</summary>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("Janitor sweep did not happen");
            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task Sweep_IntervalElapsed_EvictsIdleAndOrphanLocksKeepsFreshAndBusy()
    {
        // Проход случится в Start + 5 мин; SessionLockIdle по умолчанию 30 мин.
        var idleSince = Start + SweepInterval - TimeSpan.FromMinutes(31);

        _semaphores.Semaphores["fresh"] = new SemaphoreSlim(1, 1);
        _semaphores.LastRefreshedAt["fresh"] = Start;

        _semaphores.Semaphores["idle"] = new SemaphoreSlim(1, 1);
        _semaphores.LastRefreshedAt["idle"] = idleSince;
        _semaphores.RetryNotBefore["idle"] = idleSince.AddSeconds(15);

        // Сессия умерла до первого успешного refresh: метки времени нет.
        _semaphores.Semaphores["orphan"] = new SemaphoreSlim(1, 1);

        _semaphores.Semaphores["busy"] = new SemaphoreSlim(1, 1);
        _semaphores.LastRefreshedAt["busy"] = idleSince;
        await _semaphores.Semaphores["busy"].WaitAsync();

        using var janitor = CreateJanitor();
        await janitor.StartAsync(CancellationToken.None);

        _time.Advance(SweepInterval);
        await WaitUntilAsync(() => !_semaphores.Semaphores.ContainsKey("orphan"));

        Assert.True(_semaphores.Semaphores.ContainsKey("fresh"));
        Assert.False(_semaphores.Semaphores.ContainsKey("idle"));
        Assert.False(_semaphores.LastRefreshedAt.ContainsKey("idle"));
        Assert.False(_semaphores.RetryNotBefore.ContainsKey("idle"));
        Assert.True(_semaphores.Semaphores.ContainsKey("busy"));

        await janitor.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Sweep_IntervalNotElapsed_KeepsAllLocks()
    {
        _semaphores.Semaphores["orphan"] = new SemaphoreSlim(1, 1);

        using var janitor = CreateJanitor();
        await janitor.StartAsync(CancellationToken.None);

        _time.Advance(SweepInterval - TimeSpan.FromSeconds(1));
        await Task.Delay(100);

        Assert.True(_semaphores.Semaphores.ContainsKey("orphan"));

        await janitor.StopAsync(CancellationToken.None);
    }
}
