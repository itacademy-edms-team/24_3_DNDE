using Host.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Host.Infrastructure;

/// <summary>
/// Серверное хранилище сессии BFF.
/// Лежит на сервере, в cookie уходит только ключ сессии.
/// Текущий вид: in-memory <see cref="IDistributedCache"/>.
/// </summary>
public sealed class DistributedCacheTicketStore(
    IDistributedCache cache,
    IOptions<OidcOptions> oidc,
    TimeProvider timeProvider,
    ILogger<DistributedCacheTicketStore> logger
) : ITicketStore
{
    private const string KeyPrefix = "bff-session:";

    private readonly OidcOptions _oidc = oidc.Value;
    private readonly TimeSpan _fallbackLifetime = oidc.Value.SsoSession.SessionIdle;

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = KeyPrefix + Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        // Запись живёт ровно до конца окна сессии (ExpiresUtc), чтобы стор и cookie истекали
        // одновременно (ADR 0027). Cookie-схема проставляет ExpiresUtc до вызова стора, поэтому
        // ветка без него защитная: берём короткий обычный SSO Idle, как и запасной ExpireTimeSpan.
        var expiresUtc = ticket.Properties.ExpiresUtc ?? timeProvider.GetUtcNow() + _fallbackLifetime;
        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(expiresUtc);

        var bytes = TicketSerializer.Default.Serialize(ticket);
        await cache.SetAsync(key, bytes, options);

        // MemoryDistributedCache при переполнении молча не пишет запись: SetAsync отрабатывает без исключения, запись отсутствует.
        // Снаружи это видно только на следующем запросе, когда cookie уже выдана, а тикета в сторе нет:
        // пользователь неожиданно анонимен, и по логам непонятно почему.
        // Поэтому выполняем проверочное чтение сразу после записи.
        if (await cache.GetAsync(key) is null)
        {
            logger.LogError(
                "Session store dropped ticket {Key}: size limit {SizeLimitBytes} bytes exceeded. "
                    + "Affected user got a session cookie without a server ticket and will be anonymous on the next request",
                key,
                _oidc.SessionStoreSizeLimitBytes
            );
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await cache.GetAsync(key);
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(key);
}
