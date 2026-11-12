using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration.Sets;

/// <summary>
/// Эндпоинты наборов: контракт ошибок (ADR 0031), изоляция владельцев (404 вместо 403),
/// полная замена полей через PUT, мягкое удаление и курсорная пагинация (ADR 0039).
/// У каждого теста свой пользователь, чтобы данные не пересекались.
/// </summary>
public sealed class SetsEndpointsTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public SetsEndpointsTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task ListSets_Anonymous_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/sets");

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

        var page = await ReadPageAsync(client);
        Assert.DoesNotContain(page.Items, set => set.GetProperty("id").GetGuid() == setId);
    }

    [DatabaseFact]
    public async Task ListSets_TwoOwners_ReturnsOnlyOwnSets()
    {
        using var first = CreateClientForNewUser();
        using var second = CreateClientForNewUser();
        await CreateSetAsync(first, "Набор первого");
        var secondSetId = await CreateSetAsync(second, "Набор второго");

        var page = await ReadPageAsync(first);

        Assert.Single(page.Items);
        Assert.DoesNotContain(page.Items, set => set.GetProperty("id").GetGuid() == secondSetId);
    }

    [DatabaseFact]
    public async Task ListSets_ThreeSetsPageSizeTwo_ReturnsTwoThenOneWithoutOverlap()
    {
        using var client = CreateClientForNewUser();
        var first = await CreateSetAsync(client, "Первый");
        var second = await CreateSetAsync(client, "Второй");
        var third = await CreateSetAsync(client, "Третий");

        var firstPage = await ReadPageAsync(client, pageSize: 2);

        Assert.Equal(2, firstPage.Items.Length);
        Assert.NotNull(firstPage.NextCursor);
        // Порядок created_at DESC: последний созданный - первым в выдаче.
        Assert.Equal(third, firstPage.Items[0].GetProperty("id").GetGuid());

        var secondPage = await ReadPageAsync(client, pageSize: 2, cursor: firstPage.NextCursor);

        Assert.Single(secondPage.Items);
        Assert.Null(secondPage.NextCursor);
        Assert.Equal(first, secondPage.Items[0].GetProperty("id").GetGuid());

        var returned = firstPage.Items.Concat(secondPage.Items)
            .Select(item => item.GetProperty("id").GetGuid())
            .ToArray();
        Assert.Equal(3, returned.Distinct().Count());
        Assert.Contains(second, returned);
    }

    [DatabaseFact]
    public async Task ListSets_SameCreatedAt_UsesIdAsTieBreaker()
    {
        using var client = CreateClientForNewUser();
        var first = await CreateSetAsync(client, "Один");
        var second = await CreateSetAsync(client, "Два");

        // Одинаковая метка создания: без tie-breaker наборы могли бы попасть на обе страницы или потеряться.
        await AlignCreatedAtAsync(first, second);
        // Postgres сравнивает uuid побайтово, что для канонической записи = ordinal-сравнение hex-строки.
        // Guid.CompareTo (порядок полей, signed _a) дал бы другой результат на краевых значениях.
        var expected = new[] { first, second }
            .OrderByDescending(id => id.ToString("D"), StringComparer.Ordinal)
            .ToArray();

        var firstPage = await ReadPageAsync(client, pageSize: 1);
        var secondPage = await ReadPageAsync(client, pageSize: 1, cursor: firstPage.NextCursor);

        Assert.Equal(expected[0], firstPage.Items.Single().GetProperty("id").GetGuid());
        Assert.Equal(expected[1], secondPage.Items.Single().GetProperty("id").GetGuid());
        Assert.Null(secondPage.NextCursor);
    }

    [DatabaseFact]
    public async Task ListSets_InvalidCursor_Returns400WithCursorCode()
    {
        using var client = CreateClientForNewUser();

        var response = await client.GetAsync("/api/sets?cursor=not-a-cursor");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.CursorInvalid", problem.GetProperty("title").GetString());
        Assert.Equal("query.cursor", problem.GetProperty("target").GetString());
    }

    [DatabaseFact]
    public async Task ListSets_PageSizeOutOfRange_Returns400WithPageSizeCode()
    {
        using var client = CreateClientForNewUser();

        foreach (var pageSize in new[] { 0, 101 })
        {
            var response = await client.GetAsync($"/api/sets?pageSize={pageSize}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await ReadProblemAsync(response);
            Assert.Equal("Set.PageSizeInvalid", problem.GetProperty("title").GetString());
            Assert.Equal("query.pageSize", problem.GetProperty("target").GetString());
        }
    }

    [DatabaseFact]
    public async Task ListSets_NoSets_ReturnsEmptyPageWithNullCursor()
    {
        using var client = CreateClientForNewUser();

        var page = await ReadPageAsync(client);

        Assert.Empty(page.Items);
        Assert.Null(page.NextCursor);
    }

    [DatabaseFact]
    public async Task ListSets_ExactPageSize_ReturnsNullCursor()
    {
        using var client = CreateClientForNewUser();
        await CreateSetAsync(client, "Первый");
        await CreateSetAsync(client, "Второй");

        // Ровно страница без остатка: лишней строки не пришло и следующей страницы нет.
        var page = await ReadPageAsync(client, pageSize: 2);

        Assert.Equal(2, page.Items.Length);
        Assert.Null(page.NextCursor);
    }

    [DatabaseFact]
    public async Task ListSets_MinAndMaxPageSize_Returns200()
    {
        using var client = CreateClientForNewUser();
        await CreateSetAsync(client, "Набор");

        foreach (var pageSize in new[] { 1, 100 })
        {
            var page = await ReadPageAsync(client, pageSize: pageSize);
            Assert.Single(page.Items);
            Assert.Null(page.NextCursor);
        }
    }

    [DatabaseFact]
    public async Task ListSets_NoPageSize_ReturnsDefaultTwenty()
    {
        using var client = CreateClientForNewUser();
        for (var index = 0; index < 21; index++)
            await CreateSetAsync(client, $"Набор {index}");

        var firstPage = await ReadPageAsync(client);
        var secondPage = await ReadPageAsync(client, cursor: firstPage.NextCursor);

        Assert.Equal(20, firstPage.Items.Length);
        Assert.NotNull(firstPage.NextCursor);
        Assert.Single(secondPage.Items);
        Assert.Null(secondPage.NextCursor);
    }

    [DatabaseFact]
    public async Task ListSets_CursorWithEmptyPayload_Returns400WithCursorCode()
    {
        using var client = CreateClientForNewUser();

        // Валидный base64url и валидный JSON, но без полей: десериализуется в default-значения,
        // а такой курсор отдал бы пустую страницу вместо ошибки.
        var cursor = ToBase64Url("{}");

        var response = await client.GetAsync($"/api/sets?cursor={Uri.EscapeDataString(cursor)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.CursorInvalid", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task ListSets_RenameBetweenPages_KeepsCreatedAtOrder()
    {
        using var client = CreateClientForNewUser();
        var older = await CreateSetAsync(client, "Старый");
        var newer = await CreateSetAsync(client, "Новый");

        // Сортировка по created_at: правка не перетаскивает набор через курсор.
        await client.PutAsJsonAsync($"/api/sets/{older}", new { title = "Старый, но переименован", description = "" });

        var firstPage = await ReadPageAsync(client, pageSize: 1);
        var secondPage = await ReadPageAsync(client, pageSize: 1, cursor: firstPage.NextCursor);

        Assert.Equal(newer, firstPage.Items.Single().GetProperty("id").GetGuid());
        Assert.Equal(older, secondPage.Items.Single().GetProperty("id").GetGuid());
    }

    [DatabaseFact]
    public async Task ListSets_DeleteShownItem_NextPageHasNoGapsOrDuplicates()
    {
        using var client = CreateClientForNewUser();
        var oldest = await CreateSetAsync(client, "Первый");
        var second = await CreateSetAsync(client, "Второй");
        var third = await CreateSetAsync(client, "Третий");
        var newest = await CreateSetAsync(client, "Четвёртый");

        var firstPage = await ReadPageAsync(client, pageSize: 2);
        Assert.Equal(new[] { newest, third }, firstPage.Items.Select(item => item.GetProperty("id").GetGuid()));

        // Удаляем уже показанную строку: offset-версия «съела» бы следующую, keyset - нет.
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/sets/{newest}")).StatusCode);

        var secondPage = await ReadPageAsync(client, pageSize: 2, cursor: firstPage.NextCursor);

        Assert.Equal(new[] { second, oldest }, secondPage.Items.Select(item => item.GetProperty("id").GetGuid()));
        Assert.Null(secondPage.NextCursor);
    }

    [DatabaseFact]
    public async Task ListSets_ForeignCursor_ReturnsOnlyOwnRows()
    {
        using var stranger = CreateClientForNewUser();
        using var owner = CreateClientForNewUser();

        // Свой набор создаётся раньше чужих: тогда он попадает в диапазон «старше чужого курсора»
        // и тест доказывает именно фильтрацию по владельцу, а не пустую выдачу.
        var ownerSet = await CreateSetAsync(owner, "Свой");
        var strangerOlder = await CreateSetAsync(stranger, "Чужой старый");
        var strangerNewer = await CreateSetAsync(stranger, "Чужой новый");

        var strangerPage = await ReadPageAsync(stranger, pageSize: 1);
        Assert.Equal(strangerNewer, strangerPage.Items.Single().GetProperty("id").GetGuid());

        // Чужой курсор не раскрывает данные: фильтр по владельцу применяется до сравнения.
        var ownerPage = await ReadPageAsync(owner, cursor: strangerPage.NextCursor);

        Assert.Single(ownerPage.Items);
        var returned = ownerPage.Items.Select(item => item.GetProperty("id").GetGuid()).ToArray();
        Assert.Equal(ownerSet, Assert.Single(returned));
        Assert.DoesNotContain(strangerOlder, returned);
        Assert.DoesNotContain(strangerNewer, returned);
    }

    [DatabaseFact]
    public async Task ListSets_CursorWithoutUtcMark_Returns400WithCursorCode()
    {
        using var client = CreateClientForNewUser();

        // Время без UTC-метки: ToUniversalTime сдвинул бы его по часовому поясу сервера.
        var cursor = ToBase64Url($$"""{"CreatedAt":"2026-01-01T00:00:00","Id":"{{Guid.NewGuid()}}"}""");

        var response = await client.GetAsync($"/api/sets?cursor={Uri.EscapeDataString(cursor)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Set.CursorInvalid", problem.GetProperty("title").GetString());
    }

    private static string ToBase64Url(string value) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
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

    private static async Task<Page> ReadPageAsync(HttpClient client, int? pageSize = null, string? cursor = null)
    {
        var query = new List<string>();
        if (pageSize is not null) query.Add($"pageSize={pageSize}");
        if (cursor is not null) query.Add($"cursor={Uri.EscapeDataString(cursor)}");

        var url = query.Count == 0 ? "/api/sets" : $"/api/sets?{string.Join("&", query)}";
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new Page(
            body.GetProperty("items").EnumerateArray().ToArray(),
            body.GetProperty("nextCursor").ValueKind == JsonValueKind.Null
                ? null
                : body.GetProperty("nextCursor").GetString());
    }

    private static async Task AlignCreatedAtAsync(Guid firstId, Guid secondId)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SetsMigrations")
            ?? throw new InvalidOperationException("Set ConnectionStrings__SetsMigrations.");

        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new Npgsql.NpgsqlCommand(
            "UPDATE sets.sets SET created_at = '2026-01-01T00:00:00Z' WHERE id = ANY(@ids)",
            connection);
        command.Parameters.Add(new Npgsql.NpgsqlParameter<Guid[]>("ids", [firstId, secondId]));
        await command.ExecuteNonQueryAsync();
    }

    private sealed record Page(JsonElement[] Items, string? NextCursor);
}
