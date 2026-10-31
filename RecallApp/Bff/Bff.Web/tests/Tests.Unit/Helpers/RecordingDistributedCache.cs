using Microsoft.Extensions.Caching.Distributed;

namespace Tests.Unit.Helpers;

/// <summary>
/// IDistributedCache, который хранит записи вместе с опциями срока.
/// MemoryDistributedCache живёт по своим часам, поэтому срок записи проверяем по переданным опциям.
/// </summary>
internal sealed class RecordingDistributedCache : IDistributedCache
{
    public Dictionary<string, (byte[] Value, DistributedCacheEntryOptions Options)> Entries { get; } = [];

    /// <summary>Имитация переполнения стора: Set проходит, но запись не появляется.</summary>
    public bool DropWrites { get; set; }

    public byte[]? Get(string key) =>
        !DropWrites && Entries.TryGetValue(key, out var entry) ? entry.Value : null;

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
        Task.FromResult(Get(key));

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        if (!DropWrites)
            Entries[key] = (value, options);
    }

    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default
    )
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    public void Refresh(string key) { }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key) => Entries.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }
}
