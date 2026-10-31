using Host.Services;

namespace Tests.Unit;

/// <summary>
/// Операции с записями замков обновления токенов: удаление простаивающей записи
/// и ленивый trim с ограничением объёма уборки.
/// </summary>
public class TokenRefreshSemaphoresTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly TokenRefreshSemaphores _semaphores = new();

    private void AddSession(string key, DateTimeOffset lastRefreshedAt)
    {
        _semaphores.Semaphores[key] = new SemaphoreSlim(1, 1);
        _semaphores.LastRefreshedAt[key] = lastRefreshedAt;
        _semaphores.RetryNotBefore[key] = lastRefreshedAt.AddSeconds(15);
    }

    private bool Contains(string key) =>
        _semaphores.Semaphores.ContainsKey(key)
        || _semaphores.LastRefreshedAt.ContainsKey(key)
        || _semaphores.RetryNotBefore.ContainsKey(key);

    [Fact]
    public void TryEvict_IdleSession_RemovesFromAllMapsAndReturnsTrue()
    {
        AddSession("s1", Now);

        Assert.True(_semaphores.TryEvict("s1"));
        Assert.False(Contains("s1"));
    }

    [Fact]
    public void TryEvict_LockHeld_KeepsSessionAndReturnsFalse()
    {
        AddSession("s1", Now);
        _semaphores.Semaphores["s1"].Wait();

        Assert.False(_semaphores.TryEvict("s1"));
        Assert.True(_semaphores.Semaphores.ContainsKey("s1"));
        Assert.True(_semaphores.LastRefreshedAt.ContainsKey("s1"));
    }

    // Удаление несуществующей записи не считается ошибкой: уборщик и trim ходят по всем ключам.
    [Fact]
    public void TryEvict_UnknownKey_ReturnsTrue()
    {
        Assert.True(_semaphores.TryEvict("missing"));
    }

    [Fact]
    public void TrimOldest_ThreeSessions_RemovesTwoOldest()
    {
        AddSession("oldest", Now.AddMinutes(-30));
        AddSession("older", Now.AddMinutes(-20));
        AddSession("newest", Now);

        _semaphores.TrimOldest(2);

        Assert.False(Contains("oldest"));
        Assert.False(Contains("older"));
        Assert.True(Contains("newest"));
    }

    // Занятая запись пропускается без подстановки следующей жертвы:
    // так объём уборки не превышает maxToRemove, а занятый замок не трогаем.
    [Fact]
    public void TrimOldest_OldestLockHeld_SkipsItWithoutTakingNext()
    {
        AddSession("oldest", Now.AddMinutes(-30));
        AddSession("newest", Now);
        _semaphores.Semaphores["oldest"].Wait();

        _semaphores.TrimOldest(1);

        Assert.True(Contains("oldest"));
        Assert.True(Contains("newest"));
    }
}
