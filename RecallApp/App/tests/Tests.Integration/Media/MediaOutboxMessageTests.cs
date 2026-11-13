using System.Text.Json;
using Common.Application.Outbox;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Domain.References;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Outbox;

namespace Tests.Integration.Media;

/// <summary>
/// Сквозная доставка доменного события `MediaReferencesChanged`: outbox-строка -> десериализация
/// -> обработчик Media -> синхронизация ссылок. Проверяет и round-trip сериализации события.
/// </summary>
public sealed class MediaOutboxMessageTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public MediaOutboxMessageTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Processor_DeliversMediaReferencesChanged_SyncsReferences()
    {
        var ownerId = Guid.NewGuid();
        var elementScope = $"set:{Guid.NewGuid()}:element:{Guid.NewGuid()}";
        var mediaId = await CreateReadyMediaAsync(ownerId);

        await AddOutboxMessageAsync(new MediaReferencesChanged(
            ownerId, elementScope, [new MediaReferenceSlot($"{elementScope}:front", mediaId)]));

        await DrainAsync();

        Assert.Equal(mediaId, await ReadSlotAsync($"{elementScope}:front"));
    }

    private async Task DrainAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

        for (var pass = 0; pass < 100; pass++)
            if (await processor.ProcessBatchAsync(OutboxLanes.Fast, batchSize: 50, maxAttempts: 10, CancellationToken.None) == 0)
                return;
    }

    private async Task AddOutboxMessageAsync(MediaReferencesChanged domainEvent)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        var now = DateTime.UtcNow;

        context.OutboxMessages.Add(new OutboxMessage
        {
            Type = domainEvent.EventType,
            Version = 1,
            Lane = OutboxLanes.Fast,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            Status = OutboxStatus.Pending,
            Attempts = 0,
            NextAttemptAt = now,
            CreatedAt = now,
        });
        await context.SaveChangesAsync();
    }

    private async Task<Guid> CreateReadyMediaAsync(Guid ownerId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var now = DateTime.UtcNow;

        var created = MediaObject.Create(ownerId, MediaContentTypes.Png, now);
        Assert.False(created.IsError);
        var media = created.Value;

        var variants = new[]
        {
            MediaVariant.Create(media.Id, ownerId, MediaVariantName.Thumb, 200, 150, 600, now).Value,
            MediaVariant.Create(media.Id, ownerId, MediaVariantName.Display, 800, 600, 4_000, now).Value,
        };

        var ready = media.MarkReady(MediaFormat.Png, 800, 600, 10_000, variants, now);
        Assert.False(ready.IsError);

        context.Add(media);
        await context.SaveChangesAsync();
        return media.Id;
    }

    private async Task<Guid?> ReadSlotAsync(string reference)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        return await context.Set<MediaReference>()
            .Where(row => row.Reference == reference)
            .Select(row => (Guid?)row.MediaId)
            .SingleOrDefaultAsync();
    }
}
