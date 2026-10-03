using System.Collections.Concurrent;

namespace FinanceTrack.Gateway.Services;

public sealed class TokenExchangeSemaphores
{
    public ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; } = new();
}
