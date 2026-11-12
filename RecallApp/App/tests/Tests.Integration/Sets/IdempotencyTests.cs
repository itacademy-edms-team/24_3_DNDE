using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Idempotency;

namespace Tests.Integration.Sets;

/// <summary>
/// Идемпотентность создания набора (онтология §8): ключ и запись коммитятся вместе,
/// повтор с тем же телом отдаёт сохранённый ответ, с другим телом - 409.
/// </summary>
public sealed class IdempotencyTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public IdempotencyTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateSet_WithoutKey_Returns400WithKeyRequiredCode()
    {
        using var client = CreateClientForNewUser();

        var response = await client.PostAsJsonAsync("/api/sets", new { title = "Без ключа", description = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Idempotency.KeyRequired", problem.GetProperty("title").GetString());
        Assert.Equal("header.Idempotency-Key", problem.GetProperty("target").GetString());
    }

    [Fact]
    public async Task CreateSet_KeyIsNotUuid_Returns400WithKeyInvalidCode()
    {
        using var client = CreateClientForNewUser();

        var response = await PostSetAsync(client, "не-uuid", "Набор", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Idempotency.KeyInvalid", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task CreateSet_SameKeyAndBodyTwice_CreatesSingleSetAndReplaysResponse()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var first = await PostSetAsync(client, key, "Повтор", "тело");
        var second = await PostSetAsync(client, key, "Повтор", "тело");

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        var firstId = await ReadSetIdAsync(first);
        var secondId = await ReadSetIdAsync(second);
        Assert.Equal(firstId, secondId);

        var items = await ReadItemsAsync(client);
        Assert.Single(items);
        Assert.Equal(firstId, items[0].GetProperty("id").GetGuid());
    }

    [DatabaseFact]
    public async Task CreateSet_SameKeyDifferentBody_Returns409WithKeyReusedCode()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var first = await PostSetAsync(client, key, "Первый", null);
        var second = await PostSetAsync(client, key, "Другой", null);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var problem = await ReadProblemAsync(second);
        Assert.Equal("Idempotency.KeyReused", problem.GetProperty("title").GetString());

        var items = await ReadItemsAsync(client);
        Assert.Single(items);
    }

    [DatabaseFact]
    public async Task CreateSet_DifferentKeys_CreatesTwoSets()
    {
        using var client = CreateClientForNewUser();

        await PostSetAsync(client, Guid.NewGuid().ToString(), "Первый", null);
        await PostSetAsync(client, Guid.NewGuid().ToString(), "Второй", null);

        var items = await ReadItemsAsync(client);
        Assert.Equal(2, items.Length);
    }

    [DatabaseFact]
    public async Task CreateSet_SameKeyInDifferentUuidFormats_ReplaysResponse()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid();

        var first = await PostSetAsync(client, key.ToString("D").ToUpperInvariant(), "Набор", null);
        var second = await PostSetAsync(client, key.ToString("B"), "Набор", null);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(await ReadSetIdAsync(first), await ReadSetIdAsync(second));

        var items = await ReadItemsAsync(client);
        Assert.Single(items);
    }

    [DatabaseFact]
    public async Task CreateSet_SameKeyForDifferentOwners_CreatesSetForEach()
    {
        using var first = CreateClientForNewUser();
        using var second = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var firstResponse = await PostSetAsync(first, key, "Набор первого", null);
        var secondResponse = await PostSetAsync(second, key, "Набор второго", null);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.NotEqual(await ReadSetIdAsync(firstResponse), await ReadSetIdAsync(secondResponse));
    }

    [DatabaseFact]
    public async Task CreateSet_ParallelRequestsWithSameKey_CreateSingleSet()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var responses = await Task.WhenAll(
            PostSetAsync(client, key, "Параллельный", null),
            PostSetAsync(client, key, "Параллельный", null));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

        var ids = await Task.WhenAll(responses.Select(ReadSetIdAsync));
        Assert.Single(ids.Distinct());

        var items = await ReadItemsAsync(client);
        Assert.Single(items);
    }

    [DatabaseFact]
    public async Task CreateSet_ExpiredKey_StartsNewOperation()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var first = await PostSetAsync(client, key, "Старая операция", null);
        var firstId = await ReadSetIdAsync(first);

        // Имитируем истечение TTL: дальше ключ должен считаться новым, а не переиспользоваться.
        await ExecuteSqlAsync($"UPDATE sets.idempotency_keys SET expires_at = now() - interval '1 hour' WHERE key = '{key}'");

        var second = await PostSetAsync(client, key, "Старая операция", null);

        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var secondId = await ReadSetIdAsync(second);
        Assert.NotEqual(firstId, secondId);

        var items = await ReadItemsAsync(client);
        Assert.Equal(2, items.Length);
    }

    [DatabaseFact]
    public async Task Cleaner_ExpiredKeys_RemovesOnlyExpired()
    {
        var expiredKey = Guid.NewGuid().ToString();
        var freshKey = Guid.NewGuid().ToString();
        var ownerId = Guid.NewGuid();

        await ExecuteSqlAsync($"""
            INSERT INTO sets.idempotency_keys (owner_id, key, operation, request_hash, status_code, response, expires_at, created_at)
            VALUES ('{ownerId}', '{expiredKey}', 'sets.create', 'hash', 201, NULL, now() - interval '1 hour', now())
            """);
        await ExecuteSqlAsync($"""
            INSERT INTO sets.idempotency_keys (owner_id, key, operation, request_hash, status_code, response, expires_at, created_at)
            VALUES ('{ownerId}', '{freshKey}', 'sets.create', 'hash', 201, NULL, now() + interval '1 hour', now())
            """);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
            var deleted = await IdempotencyCleaner.DeleteExpiredAsync(
                context, DateTime.UtcNow, batchSize: 100, CancellationToken.None);

            Assert.True(deleted >= 1);
        }

        Assert.False(await KeyExistsAsync(expiredKey));
        Assert.True(await KeyExistsAsync(freshKey));

        await ExecuteSqlAsync($"DELETE FROM sets.idempotency_keys WHERE key = '{freshKey}'");
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static Task<HttpResponseMessage> PostSetAsync(HttpClient client, string key, string? title, string? description)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/sets")
        {
            Content = JsonContent.Create(new { title, description }),
        };
        request.Headers.Add("Idempotency-Key", key);
        return client.SendAsync(request);
    }

    private static async Task<Guid> ReadSetIdAsync(HttpResponseMessage response)
    {
        var set = await response.Content.ReadFromJsonAsync<JsonElement>();
        return set.GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<JsonElement[]> ReadItemsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/sets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        return page.GetProperty("items").EnumerateArray().ToArray();
    }

    private async Task ExecuteSqlAsync(string sql)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task<bool> KeyExistsAsync(string key)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.IdempotencyKeys.AnyAsync(record => record.Key == key);
    }
}
