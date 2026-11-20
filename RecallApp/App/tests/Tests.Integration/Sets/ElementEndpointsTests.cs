using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration.Sets;

/// <summary>
/// Элементы набора: единственный тип CREATE-01 (флеш-карточка), порядок рангами,
/// перестановка по соседям и курсорная выдача по позиции.
/// </summary>
public sealed class ElementEndpointsTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public ElementEndpointsTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task ListElements_Anonymous_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync($"/api/sets/{Guid.NewGuid()}/elements");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DatabaseFact]
    public async Task CreateElement_ValidFlashcard_Returns201()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term = "  serendipity  ", definition = "  счастливая случайность  " } });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("flashcard", element.GetProperty("type").GetString());
        var payload = element.GetProperty("payload");
        Assert.Equal("serendipity", payload.GetProperty("term").GetString());
        Assert.Equal("счастливая случайность", payload.GetProperty("definition").GetString());
        // Дискриминатор не дублируется в контракте: тип уже есть отдельным полем.
        Assert.False(payload.TryGetProperty("type", out _));
        Assert.Equal(
            $"/api/sets/{setId}/elements/{element.GetProperty("id").GetGuid()}",
            response.Headers.Location?.ToString());
    }

    [DatabaseFact]
    public async Task CreateElement_SecondElement_AppendsAfterFirst()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var first = await CreateElementAsync(client, setId, "Первый", "Определение 1");
        var second = await CreateElementAsync(client, setId, "Второй", "Определение 2");

        var page = await ReadPageAsync(client, setId);
        Assert.Equal(new[] { first.Id, second.Id }, page.Items.Select(IdOf));
    }

    [DatabaseFact]
    public async Task CreateElement_UnknownType_Returns400WithTypeCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "multiple-choice", payload = new { question = "a" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.TypeUnsupported", problem.GetProperty("title").GetString());
        Assert.Equal("element.type", problem.GetProperty("target").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_BlankTerm_Returns400WithTermRequiredCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term = "   ", definition = "определение" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.TermRequired", problem.GetProperty("title").GetString());
        Assert.Equal("element.payload.term", problem.GetProperty("target").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_ForeignSet_Returns404WithSetNotFoundCode()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var setId = await CreateSetAsync(owner);

        var response = await PostElementAsync(stranger,
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term = "a", definition = "b" } });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.NotFound", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task UpdateElement_NewContent_KeepsTypeAndOrder()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "Определение 1");
        var second = await CreateElementAsync(client, setId, "Второй", "Определение 2");

        var response = await client.PutAsJsonAsync(
            $"{ElementsUrl(setId)}/{second.Id}",
            new { payload = new { term = "Второй, но переписан", definition = "Новое определение" } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("flashcard", updated.GetProperty("type").GetString());
        Assert.Equal("Второй, но переписан", updated.GetProperty("payload").GetProperty("term").GetString());

        var page = await ReadPageAsync(client, setId);
        Assert.Equal(new[] { first.Id, second.Id }, page.Items.Select(IdOf));
    }

    [DatabaseFact]
    public async Task DeleteElement_MiddleElement_KeepsOrderOfRemaining()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var response = await client.DeleteAsync($"{ElementsUrl(setId)}/{second.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(new[] { first.Id, third.Id }, page.Items.Select(IdOf));
    }

    [DatabaseFact]
    public async Task MoveElement_ToFront_PutsElementBeforeFirst()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var response = await MoveAsync(client, setId, third.Id, previousId: null, nextId: first.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(new[] { third.Id, first.Id, second.Id }, page.Items.Select(IdOf));
    }

    [DatabaseFact]
    public async Task MoveElement_BetweenNeighbours_PlacesElementBetween()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var response = await MoveAsync(client, setId, third.Id, previousId: first.Id, nextId: second.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(new[] { first.Id, third.Id, second.Id }, page.Items.Select(IdOf));
    }

    [DatabaseFact]
    public async Task MoveElement_ToEnd_PutsElementLast()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var response = await MoveAsync(client, setId, first.Id, previousId: third.Id, nextId: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(new[] { second.Id, third.Id, first.Id }, page.Items.Select(IdOf));
    }

    [DatabaseFact]
    public async Task MoveElement_NoNeighbours_Returns400WithNeighboursRequiredCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var element = await CreateElementAsync(client, setId, "Первый", "1");

        var response = await MoveAsync(client, setId, element.Id, previousId: null, nextId: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.OrderNeighboursRequired", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task MoveElement_UnknownNeighbour_Returns400WithNeighbourNotFoundCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var element = await CreateElementAsync(client, setId, "Первый", "1");

        var response = await MoveAsync(client, setId, element.Id, previousId: Guid.NewGuid(), nextId: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.OrderNeighbourNotFound", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task MoveElement_ForeignSet_Returns404WithSetNotFoundCode()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var setId = await CreateSetAsync(owner);
        var element = await CreateElementAsync(owner, setId, "Первый", "1");

        var response = await MoveAsync(stranger, setId, element.Id, previousId: null, nextId: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DatabaseFact]
    public async Task MoveElement_ChangesOrderAndBumpsSetKeepsElementTimestamps()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var setBefore = await client.GetFromJsonAsync<JsonElement>($"/api/sets/{setId}");
        var before = await ReadPageAsync(client, setId);
        var beforeById = before.Items.ToDictionary(IdOf, item => item.GetProperty("updatedAt").GetDateTime());

        await MoveAsync(client, setId, first.Id, previousId: third.Id, nextId: null);

        var after = await ReadPageAsync(client, setId);
        var afterById = after.Items.ToDictionary(IdOf, item => item.GetProperty("updatedAt").GetDateTime());

        // Смена позиции - структурное изменение набора: метки элементов не двигаются.
        Assert.All(afterById, pair => Assert.Equal(beforeById[pair.Key], pair.Value));

        var setAfter = await client.GetFromJsonAsync<JsonElement>($"/api/sets/{setId}");
        Assert.True(setAfter.GetProperty("updatedAt").GetDateTime() > setBefore.GetProperty("updatedAt").GetDateTime());
    }

    [DatabaseFact]
    public async Task ListElements_PageSizeOne_ReturnsOrderWithoutOverlap()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");

        var firstPage = await ReadPageAsync(client, setId, pageSize: 1);
        var secondPage = await ReadPageAsync(client, setId, pageSize: 1, cursor: firstPage.NextCursor);

        Assert.Equal(first.Id, IdOf(firstPage.Items.Single()));
        Assert.Equal(second.Id, IdOf(secondPage.Items.Single()));
        Assert.Null(secondPage.NextCursor);
    }

    [DatabaseFact]
    public async Task ListElements_CursorWithEmptyPayload_Returns400WithElementCursorCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var cursor = ToBase64Url("{}");

        var response = await client.GetAsync($"{ElementsUrl(setId)}?cursor={Uri.EscapeDataString(cursor)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.CursorInvalid", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task DeleteSet_ThenListElements_Returns404AndHidesElements()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        await CreateElementAsync(client, setId, "Первый", "1");

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/sets/{setId}")).StatusCode);

        var response = await client.GetAsync(ElementsUrl(setId));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DatabaseFact]
    public async Task CreateElement_BumpsSetUpdatedAt()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var before = await client.GetFromJsonAsync<JsonElement>($"/api/sets/{setId}");

        await CreateElementAsync(client, setId, "Первый", "1");

        var after = await client.GetFromJsonAsync<JsonElement>($"/api/sets/{setId}");
        Assert.True(after.GetProperty("updatedAt").GetDateTime() > before.GetProperty("updatedAt").GetDateTime());
    }

    [DatabaseFact]
    public async Task CreateElement_MissingPayload_Returns400WithPayloadRequiredCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client, ElementsUrl(setId), new { type = "flashcard" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.PayloadRequired", problem.GetProperty("title").GetString());
        Assert.Equal("element.payload", problem.GetProperty("target").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_PayloadNotAnObject_Returns400WithPayloadInvalidCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "flashcard", payload = "term and definition as text" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.PayloadInvalid", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_EmptyType_Returns400WithTypeRequiredCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "", payload = new { term = "a", definition = "b" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.TypeRequired", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_TwoParallelAppends_BothCreated()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var requests = Enumerable.Range(0, 2).Select(index => PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term = $"Параллельный {index}", definition = "d" } }));

        var responses = await Task.WhenAll(requests);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

        // Оба элемента получили своё место: порядок выдаётся без дублей.
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(2, page.Items.Length);
        Assert.Equal(2, page.Items.Select(IdOf).Distinct().Count());
    }

    /// <summary>POST элемента с обязательным Idempotency-Key: каждый вызов получает свой ключ.</summary>
    private static Task<HttpResponseMessage> PostElementAsync(HttpClient client, string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return client.SendAsync(request);
    }

    private static Task<HttpResponseMessage> MoveAsync(
        HttpClient client,
        Guid setId,
        Guid elementId,
        Guid? previousId,
        Guid? nextId) =>
        client.PostAsJsonAsync(
            $"{ElementsUrl(setId)}/{elementId}/move",
            new { previousId, nextId });

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static string ElementsUrl(Guid setId) => $"/api/sets/{setId}/elements";

    private static Guid IdOf(JsonElement item) => item.GetProperty("id").GetGuid();

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

    private static async Task<Element> CreateElementAsync(
        HttpClient client,
        Guid setId,
        string term,
        string definition)
    {
        var response = await PostElementAsync(client,
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term, definition } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new Element(
            IdOf(element),
            element.GetProperty("payload").GetProperty("term").GetString()!);
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<Page> ReadPageAsync(HttpClient client, Guid setId, int? pageSize = null, string? cursor = null)
    {
        var query = new List<string>();
        if (pageSize is not null) query.Add($"pageSize={pageSize}");
        if (cursor is not null) query.Add($"cursor={Uri.EscapeDataString(cursor)}");

        var url = query.Count == 0 ? ElementsUrl(setId) : $"{ElementsUrl(setId)}?{string.Join("&", query)}";
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new Page(
            body.GetProperty("items").EnumerateArray().ToArray(),
            body.GetProperty("nextCursor").ValueKind == JsonValueKind.Null
                ? null
                : body.GetProperty("nextCursor").GetString());
    }

    private static string ToBase64Url(string value) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private sealed record Element(Guid Id, string Term);

    private sealed record Page(JsonElement[] Items, string? NextCursor);
}
