using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Host.Infrastructure;

/// <summary>
/// Серверное хранилище сессии BFF.
/// Лежит на сервере, в cookie уходит только ключ сессии.
/// Текущий вид: in-memory <see cref="IDistributedCache"/>.
/// </summary>
public sealed class DistributedCacheTicketStore(IDistributedCache cache) : ITicketStore
{
    private const string KeyPrefix = "bff-session:";

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = KeyPrefix + Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket).ConfigureAwait(false);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var options = new DistributedCacheEntryOptions();
        var expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc.HasValue)
            options.SetAbsoluteExpiration(expiresUtc.Value);
        else
            options.SetSlidingExpiration(TimeSpan.FromHours(8));

        var bytes = TicketSerializer.Default.Serialize(ticket);
        await cache.SetAsync(key, bytes, options).ConfigureAwait(false);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await cache.GetAsync(key).ConfigureAwait(false);
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(key);
}
