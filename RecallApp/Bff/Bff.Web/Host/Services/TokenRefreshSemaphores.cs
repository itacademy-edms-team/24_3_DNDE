using System.Collections.Concurrent;

namespace Host.Services;

/// <summary>
/// Разделяемое состояние сериализации обновления токенов: по одному семафору на sub
/// и время последнего успешного обновления (для двойной проверки под семафором).
/// </summary>
public sealed class TokenRefreshSemaphores
{
    public ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; } = new();
    public ConcurrentDictionary<string, DateTimeOffset> LastRefreshedAt { get; } = new();
}
