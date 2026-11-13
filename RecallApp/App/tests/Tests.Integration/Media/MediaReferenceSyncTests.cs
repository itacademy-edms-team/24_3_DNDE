using ErrorOr;
using Media.Application.References;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Domain.References;
using Media.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Media;

/// <summary>
/// Синхронизация ссылок потребителя: декларативная замена слотов области (онтология Media §6.4).
/// Ссылки пишутся в БД; хранилище не задействовано, поэтому тесты быстрые.
/// </summary>
public sealed class MediaReferenceSyncTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public MediaReferenceSyncTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Sync_NewSlots_InsertsReferences()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var front = await CreateReadyMediaAsync(ownerId);
        var back = await CreateReadyMediaAsync(ownerId);

        var result = await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", front), new MediaReferenceSlot($"{scope}:back", back)]);

        Assert.False(result.IsError);
        var slots = await ReadSlotsAsync(scope);
        Assert.Equal(2, slots.Count);
        Assert.Equal(front, slots[$"{scope}:front"]);
        Assert.Equal(back, slots[$"{scope}:back"]);
    }

    [DatabaseFact]
    public async Task Sync_ReplacingSlot_PointsItToNewFile()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var first = await CreateReadyMediaAsync(ownerId);
        var second = await CreateReadyMediaAsync(ownerId);

        await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", first)]);
        var result = await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", second)]);

        Assert.False(result.IsError);
        var slots = await ReadSlotsAsync(scope);
        Assert.Single(slots);
        Assert.Equal(second, slots[$"{scope}:front"]);
    }

    [DatabaseFact]
    public async Task Sync_DroppedSlot_RemovesReferenceOnlyInThatScope()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var otherScope = ElementScope();
        var media = await CreateReadyMediaAsync(ownerId);

        await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", media), new MediaReferenceSlot($"{scope}:back", media)]);
        await SyncAsync(ownerId, otherScope, [new MediaReferenceSlot($"{otherScope}:front", media)]);

        var result = await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", media)]);

        Assert.False(result.IsError);
        Assert.Single(await ReadSlotsAsync(scope));
        Assert.Single(await ReadSlotsAsync(otherScope));

        // Файл всё ещё используется второй областью.
        Assert.Equal(2, await CountReferencesAsync(media));
    }

    [DatabaseFact]
    public async Task Sync_EmptySet_RemovesAllSlotsOfScope()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var media = await CreateReadyMediaAsync(ownerId);
        await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", media)]);

        var result = await SyncAsync(ownerId, scope, []);

        Assert.False(result.IsError);
        Assert.Empty(await ReadSlotsAsync(scope));
        Assert.Equal(0, await CountReferencesAsync(media));
    }

    [DatabaseFact]
    public async Task Sync_NotReadyFile_ReturnsNotReadyAndKeepsState()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var pending = await CreatePendingMediaAsync(ownerId);
        await SyncAsync(ownerId, scope, []);

        var result = await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", pending)]);

        Assert.True(result.IsError);
        Assert.Equal("Media.NotReady", result.FirstError.Code);
        Assert.Empty(await ReadSlotsAsync(scope));
    }

    [DatabaseFact]
    public async Task Sync_ForeignFile_ReturnsNotFound()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var foreign = await CreateReadyMediaAsync(Guid.NewGuid());

        var result = await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{scope}:front", foreign)]);

        Assert.True(result.IsError);
        Assert.Equal("Media.NotFound", result.FirstError.Code);
    }

    [DatabaseFact]
    public async Task Sync_SlotOutsideScope_ReturnsValidationError()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var media = await CreateReadyMediaAsync(ownerId);

        var result = await SyncAsync(ownerId, scope, [new MediaReferenceSlot($"{ElementScope()}:front", media)]);

        Assert.True(result.IsError);
        Assert.Equal("Media.ReferenceInvalid", result.FirstError.Code);
    }

    [DatabaseFact]
    public async Task Sync_RepeatedSameSet_DoesNotChangeState()
    {
        var ownerId = Guid.NewGuid();
        var scope = ElementScope();
        var media = await CreateReadyMediaAsync(ownerId);
        var slots = new[] { new MediaReferenceSlot($"{scope}:front", media) };

        await SyncAsync(ownerId, scope, slots);
        var before = await ReadSlotsAsync(scope);
        await SyncAsync(ownerId, scope, slots);

        var after = await ReadSlotsAsync(scope);
        Assert.Equal(before.Count, after.Count);
        Assert.Equal(before[$"{scope}:front"], after[$"{scope}:front"]);
    }

    private static string ElementScope() => $"set:{Guid.NewGuid()}:element:{Guid.NewGuid()}";

    private async Task<ErrorOr<Success>> SyncAsync(Guid ownerId, string scope, IReadOnlyList<MediaReferenceSlot> slots)
    {
        using var serviceScope = _factory.Services.CreateScope();
        var sender = serviceScope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(new SyncReferencesCommand(ownerId, scope, slots));
    }

    private async Task<Dictionary<string, Guid>> ReadSlotsAsync(string scope)
    {
        using var serviceScope = _factory.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<MediaDbContext>();

        var rows = await context.Set<MediaReference>()
            .Where(reference => reference.Reference == scope || reference.Reference.StartsWith(scope + ":"))
            .ToArrayAsync();

        return rows.ToDictionary(reference => reference.Reference, reference => reference.MediaId, StringComparer.Ordinal);
    }

    private async Task<int> CountReferencesAsync(Guid mediaId)
    {
        using var serviceScope = _factory.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<MediaDbContext>();
        return await context.Set<MediaReference>().CountAsync(reference => reference.MediaId == mediaId);
    }

    private async Task<Guid> CreateReadyMediaAsync(Guid ownerId)
    {
        using var serviceScope = _factory.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<MediaDbContext>();

        var now = DateTime.UtcNow;
        var created = MediaObject.Create(ownerId, MediaContentTypes.Png, now);
        Assert.False(created.IsError);
        var media = created.Value;

        var variants = new[]
        {
            MediaVariant.Create(media.Id, ownerId, MediaVariantName.Thumb, 400, 300, 1_000, now).Value,
            MediaVariant.Create(media.Id, ownerId, MediaVariantName.Display, 1_000, 750, 5_000, now).Value,
        };

        var ready = media.MarkReady(MediaFormat.Png, 1_000, 750, 20_000, variants, now);
        Assert.False(ready.IsError);

        context.Add(media);
        await context.SaveChangesAsync();
        return media.Id;
    }

    private async Task<Guid> CreatePendingMediaAsync(Guid ownerId)
    {
        using var serviceScope = _factory.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<MediaDbContext>();

        var created = MediaObject.Create(ownerId, MediaContentTypes.Png, DateTime.UtcNow);
        Assert.False(created.IsError);

        context.Add(created.Value);
        await context.SaveChangesAsync();
        return created.Value.Id;
    }
}
