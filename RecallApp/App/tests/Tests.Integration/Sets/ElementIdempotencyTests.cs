using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration.Sets;

/// <summary>
/// Идемпотентность создания элемента: ключ проверяется под локом набора (внутри транзакции),
/// повтор отдаёт сохранённый ответ, другое тело - 409. Ошибка операции ключ не «съедает».
/// </summary>
public sealed class ElementIdempotencyTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public ElementIdempotencyTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateElement_WithoutKey_Returns400WithKeyRequiredCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await client.PostAsJsonAsync(
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term = "a", definition = "b" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Idempotency.KeyRequired", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_SameKeyAndBodyTwice_CreatesSingleElementAndReplaysResponse()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var key = Guid.NewGuid().ToString();
        var body = new { type = "flashcard", payload = new { term = "Повтор", definition = "тело" } };

        var first = await PostElementAsync(client, setId, key, body);
        var second = await PostElementAsync(client, setId, key, body);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        var firstId = await ReadElementIdAsync(first);
        Assert.Equal(firstId, await ReadElementIdAsync(second));

        var items = await ReadElementsAsync(client, setId);
        Assert.Single(items);
        Assert.Equal(firstId, items[0].GetProperty("id").GetGuid());
    }

    [DatabaseFact]
    public async Task CreateElement_SameKeyDifferentPayload_Returns409WithKeyReusedCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var key = Guid.NewGuid().ToString();

        var first = await PostElementAsync(client, setId, key, new { type = "flashcard", payload = new { term = "Первый", definition = "1" } });
        var second = await PostElementAsync(client, setId, key, new { type = "flashcard", payload = new { term = "Другой", definition = "2" } });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var problem = await ReadProblemAsync(second);
        Assert.Equal("Idempotency.KeyReused", problem.GetProperty("title").GetString());

        Assert.Single(await ReadElementsAsync(client, setId));
    }

    /// <summary>Неудачная попытка создания не должна сжигать ключ: следующий корректный запрос выполняется.</summary>
    [DatabaseFact]
    public async Task CreateElement_FailedAttempt_DoesNotBurnKey()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var key = Guid.NewGuid().ToString();

        // Пустой термин: операция падает, ключ должен освободиться (откат транзакции).
        var failed = await PostElementAsync(
            client, setId, key, new { type = "flashcard", payload = new { term = "   ", definition = "d" } });
        Assert.Equal(HttpStatusCode.BadRequest, failed.StatusCode);

        // Повтор с тем же ключом, но корректным телом: 201, а не 409 KeyReused.
        var retried = await PostElementAsync(
            client, setId, key, new { type = "flashcard", payload = new { term = "Позиция", definition = "d" } });
        Assert.Equal(HttpStatusCode.Created, retried.StatusCode);

        Assert.Single(await ReadElementsAsync(client, setId));
    }

    [DatabaseFact]
    public async Task CreateElement_ParallelRequestsWithSameKey_CreateSingleElement()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var key = Guid.NewGuid().ToString();
        var body = new { type = "flashcard", payload = new { term = "Параллельный", definition = "d" } };

        var responses = await Task.WhenAll(
            PostElementAsync(client, setId, key, body),
            PostElementAsync(client, setId, key, body));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

        var ids = await Task.WhenAll(responses.Select(ReadElementIdAsync));
        Assert.Single(ids.Distinct());
        Assert.Single(await ReadElementsAsync(client, setId));
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

    private static Task<HttpResponseMessage> PostElementAsync(HttpClient client, Guid setId, string key, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ElementsUrl(setId))
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("Idempotency-Key", key);
        return client.SendAsync(request);
    }

    private static async Task<Guid> ReadElementIdAsync(HttpResponseMessage response)
    {
        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        return element.GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<JsonElement[]> ReadElementsAsync(HttpClient client, Guid setId)
    {
        var response = await client.GetAsync(ElementsUrl(setId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        return page.GetProperty("items").EnumerateArray().ToArray();
    }
}
