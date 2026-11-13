using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Purge;

namespace Tests.Integration.Sets;

/// <summary>
/// Физическая чистка мягко удалённого контента после окна retention (ADR 0036, ступень 3):
/// просроченные tombstone удаляются, свежие остаются.
/// </summary>
public sealed class PurgeTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public PurgeTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Purge_ExpiredSet_RemovesSetAndItsElements()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        await CreateElementAsync(client, setId, "Первый", "1");
        await CreateElementAsync(client, setId, "Второй", "2");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/sets/{setId}")).StatusCode);

        await ExecuteSqlAsync($"""
            UPDATE sets.sets SET deleted_at = now() - interval '40 days' WHERE id = '{setId}';
            UPDATE sets.elements SET deleted_at = now() - interval '40 days' WHERE set_id = '{setId}';
            """);

        var (sets, _) = await PurgeAsync();

        // Элементы удалены каскадом FK вместе с набором, поэтому второй проход чистильщика их не считает.
        Assert.True(sets >= 1);
        Assert.False(await SetExistsAsync(setId));
        Assert.Equal(0, await ElementsCountAsync(setId));
    }

    [DatabaseFact]
    public async Task Purge_FreshTombstone_KeepsSetAndElements()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        await CreateElementAsync(client, setId, "Первый", "1");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/sets/{setId}")).StatusCode);

        // Tombstone моложе retention: чистить нечего.
        await PurgeAsync();

        Assert.True(await SetExistsAsync(setId));
        Assert.Equal(1, await ElementsCountAsync(setId));
    }

    [DatabaseFact]
    public async Task Purge_ExpiredElementInLiveSet_RemovesOnlyElement()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var removed = await CreateElementAsync(client, setId, "Удаляемый", "1");
        var kept = await CreateElementAsync(client, setId, "Оставшийся", "2");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{ElementsUrl(setId)}/{removed}")).StatusCode);

        await ExecuteSqlAsync($"""
            UPDATE sets.elements SET deleted_at = now() - interval '40 days' WHERE id = '{removed}';
            """);

        await PurgeAsync();

        Assert.True(await SetExistsAsync(setId));
        Assert.Equal(1, await ElementsCountAsync(setId));
        var items = await ReadElementsAsync(client, setId);
        Assert.Equal(kept, Assert.Single(items).GetProperty("id").GetGuid());
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static string ElementsUrl(Guid setId) => $"/api/sets/{setId}/elements";

    private static async Task<Guid> CreateSetAsync(HttpClient client)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/sets")
        {
            Content = JsonContent.Create(new { title = "Набор", description = (string?)null }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var set = await response.Content.ReadFromJsonAsync<JsonElement>();
        return set.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateElementAsync(HttpClient client, Guid setId, string term, string definition)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ElementsUrl(setId))
        {
            Content = JsonContent.Create(new { type = "flashcard", payload = new { term, definition } }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        return element.GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement[]> ReadElementsAsync(HttpClient client, Guid setId)
    {
        var response = await client.GetAsync(ElementsUrl(setId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        return page.GetProperty("items").EnumerateArray().ToArray();
    }

    private async Task<(int Sets, int Elements)> PurgeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();

        return await SetsPurger.PurgeAsync(
            context,
            DateTime.UtcNow.AddDays(-30),
            batchSize: 100,
            CancellationToken.None);
    }

    private async Task ExecuteSqlAsync(string sql)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task<bool> SetExistsAsync(Guid setId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.Sets.AnyAsync(set => set.Id == setId);
    }

    private async Task<int> ElementsCountAsync(Guid setId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.Elements.CountAsync(element => element.SetId == setId);
    }
}
