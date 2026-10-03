using System.Collections.Concurrent;

namespace FinanceTrack.Gateway.Services;

public sealed class TokenRefreshSemaphores
{
    public ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; } = new();
    public ConcurrentDictionary<string, DateTimeOffset> LastRefreshedAt { get; } = new();
}
