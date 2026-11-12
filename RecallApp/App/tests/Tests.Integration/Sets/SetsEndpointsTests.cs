using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration.Sets;

/// <summary>
/// Тесты эндпоинтов набора: контракт ошибок (ADR 0031),
/// изоляция владельцев (404 вместо 403),
/// полная замена полей через PUT и мягкое удаление.
/// У каждого теста свой пользователь, чтобы данные не пересекались.
/// </summary>
public sealed class SetsEndpointsTests(AppFactory factory) : IClassFixture<AppFactory>
{
    [Fact]
    public async Task ListSets_Anonymous_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/sets");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DatabaseFact]
    public async Task CreateSet_ValidRequest_Returns201WithTrimmedTitleAndLocation()
    {
        using var client = CreateClientForNewUser();

        var response = await client.PostAsJsonAsync("/api/sets", new { title = "  Мой набор  ", description = "  описание  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var set = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Мой набор", set.GetProperty("title").GetString());
        Assert.Equal("описание", set.GetProperty("description").GetString());
        Assert.Equal($"/api/sets/{set.GetProperty("id").GetGuid()}", response.Headers.Location?.ToString());
    }

    [DatabaseFact]
    public async Task CreateSet_BlankTitle_Returns400WithDomainCode()
    {
        using var client = CreateClientForNewUser();

        var response = await client.PostAsJsonAsync("/api/sets", new { title = "   ", description = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.TitleRequired", problem.GetProperty("title").GetString());
        Assert.Equal("app", problem.GetProperty("source").GetString());
        Assert.Equal("set.title", problem.GetProperty("target").GetString());
    }

    [DatabaseFact]
    public async Task CreateSet_TitleTooLong_Returns400WithValidatorCode()
    {
        using var client = CreateClientForNewUser();
        var tooLong = new string('a', 121);

        var response = await client.PostAsJsonAsync("/api/sets", new { title = tooLong, description = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.TitleTooLong", problem.GetProperty("title").GetString());
        Assert.Equal("set.title", problem.GetProperty("target").GetString());
    }

    [DatabaseFact]
    public async Task GetSet_ForeignOwner_Returns404WithNotFoundCode()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var setId = await CreateSetAsync(owner, "Чужой набор");

        var response = await stranger.GetAsync($"/api/sets/{setId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.NotFound", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task UpdateSet_EmptyDescription_Returns200AndClearsDescription()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client, "Старое имя", "старое описание");

        var response = await client.PutAsJsonAsync($"/api/sets/{setId}", new { title = "Новое имя", description = "" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Новое имя", updated.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, updated.GetProperty("description").ValueKind);

        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/sets/{setId}");
        Assert.Equal("Новое имя", reread.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, reread.GetProperty("description").ValueKind);
    }

    [DatabaseFact]
    public async Task DeleteSet_ThenGetAndList_ExcludesSet()
    {
        using var client = CreateClientForNewUser();
        var setId = await CreateSetAsync(client, "Удаляемый набор");

        var deleteResponse = await client.DeleteAsync($"/api/sets/{setId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/sets/{setId}")).StatusCode);

        var list = await client.GetFromJsonAsync<JsonElement>("/api/sets");
        Assert.DoesNotContain(list.EnumerateArray(), set => set.GetProperty("id").GetGuid() == setId);
    }

    [DatabaseFact]
    public async Task ListSets_TwoOwners_ReturnsOnlyOwnSets()
    {
        using var first = CreateClientForNewUser();
        using var second = CreateClientForNewUser();
        await CreateSetAsync(first, "Набор первого");
        var secondSetId = await CreateSetAsync(second, "Набор второго");

        var firstList = await first.GetFromJsonAsync<JsonElement>("/api/sets");

        Assert.Single(firstList.EnumerateArray());
        Assert.DoesNotContain(firstList.EnumerateArray(), set => set.GetProperty("id").GetGuid() == secondSetId);
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static async Task<Guid> CreateSetAsync(HttpClient client, string title, string? description = null)
    {
        var response = await client.PostAsJsonAsync("/api/sets", new { title, description });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var set = await response.Content.ReadFromJsonAsync<JsonElement>();
        return set.GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
