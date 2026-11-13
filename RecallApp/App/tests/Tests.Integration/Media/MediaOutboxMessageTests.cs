using System.Text.Json;
using Common.Application.Outbox;
using ErrorOr;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Domain.References;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Media;

/// <summary>
/// Приёмная сторона outbox: сообщение `media.references.sync` разбирается через общий контракт
/// и применяется к ссылкам. Проверяется именно адаптер, а не сценарий синхронизации.
/// </summary>
public sealed class MediaOutboxMessageTests : IClassFixture<AppFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppFactory _factory;

    public MediaOutboxMessageTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task ReferencesSyncMessage_AppliesSlotsFromPayload()
    {
        var ownerId = Guid.NewGuid();
        var scope = $"set:{Guid.NewGuid()}:element:{Guid.NewGuid()}";
        var mediaId = await CreateReadyMediaAsync(ownerId);

        var payload = new MediaReferencesSyncPayload(
            ownerId,
            scope,
            [new MediaReferenceSlot($"{scope}:front", mediaId)]);

        var result = await HandleAsync(payload);

        Assert.False(result.IsError);
        Assert.Equal(mediaId, await ReadSlotAsync($"{scope}:front"));
    }

    [DatabaseFact]
    public async Task ReferencesSyncMessage_EmptySlots_RemovesReferences()
    {
        var ownerId = Guid.NewGuid();
        var scope = $"set:{Guid.NewGuid()}:element:{Guid.NewGuid()}";
        var mediaId = await CreateReadyMediaAsync(ownerId);

        await HandleAsync(new MediaReferencesSyncPayload(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", mediaId)]));
        var result = await HandleAsync(new MediaReferencesSyncPayload(ownerId, scope, []));

        Assert.False(result.IsError);
        Assert.Null(await ReadSlotAsync($"{scope}:front"));
    }

    [DatabaseFact]
    public async Task ReferencesSyncMessage_InvalidPayload_ReturnsMessageInvalid()
    {
        using var providerScope = _factory.Services.CreateScope();
        var handlers = providerScope.ServiceProvider.GetServices<IOutboxMessageHandler>();
        var handler = Assert.Single(handlers, candidate => candidate.MessageType == MediaOutboxMessages.ReferencesSync);

        using var document = JsonDocument.Parse("""{"ownerId":"00000000-0000-0000-0000-000000000000","elementScope":""}""");
        var result = await handler.HandleAsync(document.RootElement, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Media.MessageInvalid", result.FirstError.Code);
    }

    [Fact]
    public void ReferencesSyncHandler_MessageType_MatchesContract()
    {
        using var providerScope = _factory.Services.CreateScope();
        var handler = providerScope.ServiceProvider.GetServices<IOutboxMessageHandler>()
            .SingleOrDefault(candidate => candidate.MessageType == MediaOutboxMessages.ReferencesSync);

        Assert.NotNull(handler);
    }

    private async Task<ErrorOr<Success>> HandleAsync(MediaReferencesSyncPayload payload)
    {
        using var providerScope = _factory.Services.CreateScope();
        var handler = providerScope.ServiceProvider.GetServices<IOutboxMessageHandler>()
            .Single(candidate => candidate.MessageType == MediaOutboxMessages.ReferencesSync);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload, JsonOptions));
        return await handler.HandleAsync(document.RootElement, CancellationToken.None);
    }

    private async Task<Guid?> ReadSlotAsync(string reference)
    {
        using var providerScope = _factory.Services.CreateScope();
        var context = providerScope.ServiceProvider.GetRequiredService<MediaDbContext>();
        return await context.Set<MediaReference>()
            .Where(row => row.Reference == reference)
            .Select(row => (Guid?)row.MediaId)
            .SingleOrDefaultAsync();
    }

    private async Task<Guid> CreateReadyMediaAsync(Guid ownerId)
    {
        using var providerScope = _factory.Services.CreateScope();
        var context = providerScope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var now = DateTime.UtcNow;

        var created = MediaObject.Create(ownerId, MediaContentTypes.Png, now);
        Assert.False(created.IsError);
        var media = created.Value;

        var variants = new[]
        {
            MediaVariant.Create(media.Id, ownerId, MediaVariantName.Thumb, 200, 150, 800, now).Value,
            MediaVariant.Create(media.Id, ownerId, MediaVariantName.Display, 640, 480, 3_000, now).Value,
        };

        var ready = media.MarkReady(MediaFormat.Png, 640, 480, 10_000, variants, now);
        Assert.False(ready.IsError);

        context.Add(media);
        await context.SaveChangesAsync();
        return media.Id;
    }
}
