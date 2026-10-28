using System.Security.Claims;
using Host.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Time.Testing;
using Tests.Unit.Helpers;

namespace Tests.Unit;

public class DistributedCacheTicketStoreTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly RecordingDistributedCache _cache = new();
    private readonly DistributedCacheTicketStore _store;

    public DistributedCacheTicketStoreTests()
    {
        _store = new DistributedCacheTicketStore(_cache, TestOidcOptions.Wrap(), new FakeTimeProvider(Now));
    }

    private static AuthenticationTicket CreateTicket(DateTimeOffset? expiresUtc)
    {
        var identity = new ClaimsIdentity([new Claim("sub", "user-1")], "Test");
        var properties = new AuthenticationProperties { ExpiresUtc = expiresUtc };
        properties.SetString("bff_session_id", "session-1");
        return new AuthenticationTicket(new ClaimsPrincipal(identity), properties, "Cookies");
    }

    [Fact]
    public async Task StoreAsync_TwoTickets_ReturnsDistinctPrefixedKeys()
    {
        var first = await _store.StoreAsync(CreateTicket(Now.AddMinutes(30)));
        var second = await _store.StoreAsync(CreateTicket(Now.AddMinutes(30)));

        Assert.StartsWith("bff-session:", first);
        Assert.NotEqual(first, second);
        Assert.Equal(2, _cache.Entries.Count);
    }

    [Fact]
    public async Task RetrieveAsync_StoredKey_ReturnsTicket()
    {
        var key = await _store.StoreAsync(CreateTicket(Now.AddMinutes(30)));

        var ticket = await _store.RetrieveAsync(key);

        Assert.NotNull(ticket);
        Assert.Equal("user-1", ticket.Principal.FindFirst("sub")?.Value);
        Assert.Equal("session-1", ticket.Properties.GetString("bff_session_id"));
    }

    [Fact]
    public async Task RetrieveAsync_UnknownKey_ReturnsNull()
    {
        Assert.Null(await _store.RetrieveAsync("bff-session:missing"));
    }

    [Fact]
    public async Task StoreAsync_TicketWithExpiresUtc_SetsEntryExpirationToExpiresUtc()
    {
        var expiresUtc = Now.AddMinutes(30);

        var key = await _store.StoreAsync(CreateTicket(expiresUtc));

        Assert.Equal(expiresUtc, _cache.Entries[key].Options.AbsoluteExpiration);
    }

    [Fact]
    public async Task RenewAsync_NewExpiresUtc_MovesEntryExpiration()
    {
        var key = await _store.StoreAsync(CreateTicket(Now.AddMinutes(30)));
        var renewedUntil = Now.AddMinutes(45);

        await _store.RenewAsync(key, CreateTicket(renewedUntil));

        Assert.Equal(renewedUntil, _cache.Entries[key].Options.AbsoluteExpiration);
    }

    [Fact]
    public async Task StoreAsync_TicketWithoutExpiresUtc_SetsEntryExpirationToRegularSsoIdle()
    {
        var key = await _store.StoreAsync(CreateTicket(expiresUtc: null));

        Assert.Equal(
            Now + TestOidcOptions.Create().SsoSession.SessionIdle,
            _cache.Entries[key].Options.AbsoluteExpiration
        );
    }

    [Fact]
    public async Task RemoveAsync_StoredKey_DeletesEntry()
    {
        var key = await _store.StoreAsync(CreateTicket(Now.AddMinutes(30)));

        await _store.RemoveAsync(key);

        Assert.Null(await _store.RetrieveAsync(key));
    }
}
