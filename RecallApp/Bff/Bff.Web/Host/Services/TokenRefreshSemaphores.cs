using System.Collections.Concurrent;

namespace Host.Services;

/// <summary>
/// Разделяемое состояние сериализации обновления токенов: по одному семафору на BFF-сессию.
/// Время последнего успешного обновления (для двойной проверки под семафором)
/// и момент, раньше которого не повторять refresh после временного сбоя Keycloak.
/// Ключ - идентификатор BFF-сессии (<see cref="BffSessionKeys.SessionId"/>), не sub:
/// цепочка refresh живёт на сессию, не на пользователя.
/// Записи мёртвых сессий чистит <see cref="TokenRefreshSessionJanitor"/> плюс ленивый trim.
/// </summary>
public sealed class TokenRefreshSemaphores
{
    /// <summary>Семафор на BFF-сессию: сериализует обновление её цепочки токенов.</summary>
    public ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; } = new();

    /// <summary>Время последнего успешного refresh по сессии (двойная проверка под семафором).</summary>
    public ConcurrentDictionary<string, DateTimeOffset> LastRefreshedAt { get; } = new();

    /// <summary>Момент, раньше которого не повторять refresh после временного сбоя Keycloak.</summary>
    public ConcurrentDictionary<string, DateTimeOffset> RetryNotBefore { get; } = new();

    /// <summary>
    /// Удаляет запись замка сессии, если он прямо сейчас не занят. Семафор не Dispose-им:
    /// запрос, уже захвативший ссылку через GetOrAdd, продолжит работать корректно, а GC
    /// заберёт объект, когда на него не останется ссылок. Так исключается гонка с Dispose.
    /// </summary>
    /// <returns>Возвращает true, если запись удалена; false, если замок занят.</returns>
    public bool TryEvict(string key)
    {
        if (Semaphores.TryGetValue(key, out var sem) && sem.CurrentCount == 0)
            return false; // занят - кто-то обновляет прямо сейчас

        LastRefreshedAt.TryRemove(key, out _);
        RetryNotBefore.TryRemove(key, out _);
        Semaphores.TryRemove(key, out _);
        return true;
    }

    /// <summary>
    /// Ленивый guard: выметает до <paramref name="maxToRemove"/> самых старых простаивающих записей.
    /// Страховка от всплеска логинов между проходами уборщика, объём уборки ограничен.
    /// </summary>
    public void TrimOldest(int maxToRemove)
    {
        var victims = LastRefreshedAt
            .OrderBy(kvp => kvp.Value)
            .Take(maxToRemove)
            .Select(kvp => kvp.Key)
            .ToArray();

        foreach (var key in victims)
            TryEvict(key);
    }
}
