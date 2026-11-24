using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Accounts.Infrastructure.Data;
using Media.Domain.Objects;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Outbox;

namespace Tests.Integration.Sets;

/// <summary>
/// Изображения в элементах: синхронная проверка готовности файла (Media.Contracts), ссылки в ответе
/// и сообщения outbox для учёта использования (ADR 0040, онтология Media §7.4).
/// </summary>
public sealed class ElementImagesTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public ElementImagesTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task CreateElement_WithReadyImage_EchoesIdAndEnqueuesSync()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);
        var mediaId = await CreateMediaAsync(owner, ready: true);

        var response = await PostElementAsync(client, setId, "Термин", "Определение", mediaId);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(mediaId, element.GetProperty("payload").GetProperty("frontImageId").GetGuid());

        var elementScope = $"set:{setId}:element:{element.GetProperty("id").GetGuid()}";
        var message = await LastMessageAsync(elementScope);
        Assert.Equal("media.references.sync", message.Type);

        var ownerId = await ResolveOwnerIdAsync(owner);
        using var payload = JsonDocument.Parse(message.Payload);
        Assert.Equal(ownerId, payload.RootElement.GetProperty("ownerId").GetGuid());
        var slot = payload.RootElement.GetProperty("slots").EnumerateArray().Single();
        Assert.Equal($"{elementScope}:front", slot.GetProperty("reference").GetString());
        Assert.Equal(mediaId, slot.GetProperty("mediaId").GetGuid());
    }

    [DatabaseFact]
    public async Task CreateElement_WithoutImages_EnqueuesEmptySlotSet()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);

        var response = await PostElementAsync(client, setId, "Термин", "Определение", null);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        var message = await LastMessageAsync($"set:{setId}:element:{element.GetProperty("id").GetGuid()}");
        using var payload = JsonDocument.Parse(message.Payload);
        Assert.Empty(payload.RootElement.GetProperty("slots").EnumerateArray());
    }

    [DatabaseFact]
    public async Task CreateElement_WithPendingImage_Returns409NotReady()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);
        var mediaId = await CreateMediaAsync(owner, ready: false);

        var response = await PostElementAsync(client, setId, "Термин", "Определение", mediaId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Media.NotReady", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task CreateElement_WithForeignImage_Returns404()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);
        var foreignOwner = NewOwner();
        using var foreignClient = CreateClient(foreignOwner);
        await CreateSetAsync(foreignClient);
        var foreignMedia = await CreateMediaAsync(foreignOwner, ready: true);

        var response = await PostElementAsync(client, setId, "Термин", "Определение", foreignMedia);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Media.NotFound", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task ListElements_WithImage_ReturnsSignedLinks()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);
        var mediaId = await CreateMediaAsync(owner, ready: true);
        await PostElementAsync(client, setId, "Термин", "Определение", mediaId);

        var list = await client.GetAsync($"/api/sets/{setId}/elements");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var page = await list.Content.ReadFromJsonAsync<JsonElement>();
        var item = page.GetProperty("items").EnumerateArray().Single();
        var thumb = item.GetProperty("images").GetProperty("front").GetProperty("thumb");
        Assert.False(string.IsNullOrWhiteSpace(thumb.GetProperty("url").GetString()));
        Assert.Equal(200, thumb.GetProperty("width").GetInt32());
    }

    [DatabaseFact]
    public async Task DeleteElement_EnqueuesEmptySlotSet()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);
        var mediaId = await CreateMediaAsync(owner, ready: true);
        var created = await PostElementAsync(client, setId, "Термин", "Определение", mediaId);
        var element = await created.Content.ReadFromJsonAsync<JsonElement>();
        var elementId = element.GetProperty("id").GetGuid();

        var deleted = await client.DeleteAsync($"/api/sets/{setId}/elements/{elementId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var message = await LastMessageAsync($"set:{setId}:element:{elementId}");
        using var payload = JsonDocument.Parse(message.Payload);
        Assert.Empty(payload.RootElement.GetProperty("slots").EnumerateArray());
    }

    [DatabaseFact]
    public async Task DeleteSet_EnqueuesScopeLevelRemoval()
    {
        var owner = NewOwner();
        using var client = CreateClient(owner);
        var setId = await CreateSetAsync(client);
        var mediaId = await CreateMediaAsync(owner, ready: true);
        await PostElementAsync(client, setId, "Термин", "Определение", mediaId);

        var deleted = await client.DeleteAsync($"/api/sets/{setId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var message = await LastMessageAsync($"set:{setId}\"");
        using var payload = JsonDocument.Parse(message.Payload);
        Assert.Equal($"set:{setId}", payload.RootElement.GetProperty("elementScope").GetString());
        Assert.Empty(payload.RootElement.GetProperty("slots").EnumerateArray());
    }

    private static Guid NewOwner() => Guid.NewGuid();

    private HttpClient CreateClient(Guid ownerId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, ownerId.ToString());
        return client;
    }

    private static async Task<Guid> CreateSetAsync(HttpClient client)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/sets")
        {
            Content = JsonContent.Create(new { title = "Набор с картинками" }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private static Task<HttpResponseMessage> PostElementAsync(
        HttpClient client,
        Guid setId,
        string term,
        string definition,
        Guid? frontImageId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/sets/{setId}/elements")
        {
            Content = JsonContent.Create(new
            {
                type = "flashcard",
                payload = new { term, definition, frontImageId, backImageId = (Guid?)null },
            }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return client.SendAsync(request);
    }

    private async Task<Guid> CreateMediaAsync(Guid ownerSub, bool ready)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var now = DateTime.UtcNow;

        // Медиа принадлежит доменному пользователю, а не значению sub из токена:
        // JIT-провижининг выдаёт свой идентификатор, его и надо использовать как владельца.
        var ownerId = await ResolveOwnerIdAsync(ownerSub);

        var created = MediaObject.Create(ownerId, MediaContentTypes.Png, now);
        Assert.False(created.IsError);
        var media = created.Value;

        if (ready)
        {
            var variants = new[]
            {
                MediaVariant.Create(media.Id, ownerId, MediaVariantName.Thumb, 200, 150, 600, now).Value,
                MediaVariant.Create(media.Id, ownerId, MediaVariantName.Display, 800, 600, 4_000, now).Value,
            };

            var marked = media.MarkReady(MediaFormat.Png, 800, 600, 10_000, variants, now);
            Assert.False(marked.IsError);
        }

        context.Add(media);
        await context.SaveChangesAsync();
        return media.Id;
    }

    private async Task<OutboxMessage[]> MessagesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages.OrderBy(message => message.Id).ToArrayAsync();
    }

    /// <summary>Сообщения ищутся по вхождению области в payload и берётся последнее: таблица общая для прогона.</summary>
    private async Task<OutboxMessage> LastMessageAsync(string scopeMarker)
    {
        var messages = await MessagesAsync();
        return messages
            .Where(message => message.Payload.Contains(scopeMarker, StringComparison.Ordinal))
            .OrderBy(message => message.Id)
            .Last();
    }

    /// <summary>Доменный идентификатор по sub: JIT-провижининг выдаёт свой id, отличный от токена.</summary>
    private async Task<Guid> ResolveOwnerIdAsync(Guid keycloakSub)
    {
        using var scope = _factory.Services.CreateScope();
        var accounts = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        return await accounts.Users
            .Where(user => user.KeycloakSub == keycloakSub)
            .Select(user => user.Id)
            .SingleAsync();
    }
}
