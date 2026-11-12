using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration.Sets;

/// <summary>
/// Элементы набора: единственный тип CREATE-01 (флеш-карточка), плотные позиции,
/// полная перестановка с 409 при расхождении состава, курсорная выдача по позиции.
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
    public async Task CreateElement_ValidFlashcard_Returns201WithPositionZero()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await client.PostAsJsonAsync(
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
        Assert.Equal(0, element.GetProperty("position").GetInt32());
        Assert.Equal(
            $"/api/sets/{setId}/elements/{element.GetProperty("id").GetGuid()}",
            response.Headers.Location?.ToString());
    }

    [DatabaseFact]
    public async Task CreateElement_SecondElement_GetsNextPosition()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var first = await CreateElementAsync(client, setId, "Первый", "Определение 1");
        var second = await CreateElementAsync(client, setId, "Второй", "Определение 2");

        Assert.Equal(0, first.Position);
        Assert.Equal(1, second.Position);
    }

    [DatabaseFact]
    public async Task CreateElement_UnknownType_Returns400WithTypeCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);

        var response = await client.PostAsJsonAsync(
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

        var response = await client.PostAsJsonAsync(
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

        var response = await stranger.PostAsJsonAsync(
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term = "a", definition = "b" } });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.NotFound", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task UpdateElement_NewContent_KeepsTypeAndPosition()
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
        Assert.Equal(second.Position, updated.GetProperty("position").GetInt32());
        Assert.Equal("flashcard", updated.GetProperty("type").GetString());
        Assert.Equal("Второй, но переписан", updated.GetProperty("payload").GetProperty("term").GetString());
        Assert.Equal("Новое определение", updated.GetProperty("payload").GetProperty("definition").GetString());

        // Первый элемент не затронут.
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(first.Term, page.Items[0].GetProperty("payload").GetProperty("term").GetString());
    }

    [DatabaseFact]
    public async Task DeleteElement_MiddlePosition_CompactsRemaining()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var response = await client.DeleteAsync($"{ElementsUrl(setId)}/{second.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(
            new[] { first.Id, third.Id },
            page.Items.Select(item => item.GetProperty("id").GetGuid()));
        Assert.Equal(new[] { 0, 1 }, page.Items.Select(item => item.GetProperty("position").GetInt32()));
    }

    [DatabaseFact]
    public async Task ReorderElements_ReversedOrder_ReturnsNewPositions()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");
        var third = await CreateElementAsync(client, setId, "Третий", "3");

        var response = await client.PutAsJsonAsync(
            $"{ElementsUrl(setId)}/order",
            new { elementIds = new[] { third.Id, second.Id, first.Id } });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var page = await ReadPageAsync(client, setId);
        Assert.Equal(
            new[] { third.Id, second.Id, first.Id },
            page.Items.Select(item => item.GetProperty("id").GetGuid()));
        Assert.Equal(new[] { 0, 1, 2 }, page.Items.Select(item => item.GetProperty("position").GetInt32()));
    }

    [DatabaseFact]
    public async Task ReorderElements_MissingElement_Returns409WithOrderMismatchCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        _ = await CreateElementAsync(client, setId, "Второй", "2");

        var response = await client.PutAsJsonAsync(
            $"{ElementsUrl(setId)}/order",
            new { elementIds = new[] { first.Id } });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.OrderMismatch", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task ReorderElements_DuplicateId_Returns409WithOrderMismatchCode()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        _ = await CreateElementAsync(client, setId, "Второй", "2");

        var response = await client.PutAsJsonAsync(
            $"{ElementsUrl(setId)}/order",
            new { elementIds = new[] { first.Id, first.Id } });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.OrderMismatch", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task ListElements_PageSizeOne_ReturnsPositionsWithoutOverlap()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client);
        var first = await CreateElementAsync(client, setId, "Первый", "1");
        var second = await CreateElementAsync(client, setId, "Второй", "2");

        var firstPage = await ReadPageAsync(client, setId, pageSize: 1);
        var secondPage = await ReadPageAsync(client, setId, pageSize: 1, cursor: firstPage.NextCursor);

        Assert.Equal(first.Id, firstPage.Items.Single().GetProperty("id").GetGuid());
        Assert.Equal(second.Id, secondPage.Items.Single().GetProperty("id").GetGuid());
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

        var response = await client.PostAsJsonAsync(ElementsUrl(setId), new { type = "flashcard" });

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

        var response = await client.PostAsJsonAsync(
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

        var response = await client.PostAsJsonAsync(
            ElementsUrl(setId),
            new { type = "", payload = new { term = "a", definition = "b" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Element.TypeRequired", problem.GetProperty("title").GetString());
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
        var response = await client.PostAsJsonAsync("/api/sets", new { title = "Набор", description = (string?)null });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var set = await response.Content.ReadFromJsonAsync<JsonElement>();
        return set.GetProperty("id").GetGuid();
    }

    private static async Task<Element> CreateElementAsync(HttpClient client, Guid setId, string term, string definition)
    {
        var response = await client.PostAsJsonAsync(
            ElementsUrl(setId),
            new { type = "flashcard", payload = new { term, definition } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new Element(
            element.GetProperty("id").GetGuid(),
            element.GetProperty("position").GetInt32(),
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

    private sealed record Element(Guid Id, int Position, string Term);

    private sealed record Page(JsonElement[] Items, string? NextCursor);
}
