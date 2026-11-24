using ErrorOr;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Media;

/// <summary>
/// Контракт Media для других модулей: проверка «файл готов и мой» перед сохранением
/// и выдача подписанных ссылок при чтении (онтология Media §7.4, §8.2).
/// </summary>
public sealed class MediaContractTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public MediaContractTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task EnsureReady_ReadyOwnedMedia_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var mediaId = await CreateMediaAsync(ownerId, ready: true);

        var result = await ReadinessAsync(ownerId, [mediaId]);

        Assert.False(result.IsError);
    }

    [DatabaseFact]
    public async Task EnsureReady_EmptySet_Succeeds()
    {
        var result = await ReadinessAsync(Guid.NewGuid(), []);

        Assert.False(result.IsError);
    }

    [DatabaseFact]
    public async Task EnsureReady_PendingMedia_ReturnsNotReady()
    {
        var ownerId = Guid.NewGuid();
        var mediaId = await CreateMediaAsync(ownerId, ready: false);

        var result = await ReadinessAsync(ownerId, [mediaId]);

        Assert.True(result.IsError);
        Assert.Equal("Media.NotReady", result.FirstError.Code);
    }

    [DatabaseFact]
    public async Task EnsureReady_ForeignMedia_ReturnsNotFound()
    {
        var mediaId = await CreateMediaAsync(Guid.NewGuid(), ready: true);

        var result = await ReadinessAsync(Guid.NewGuid(), [mediaId]);

        Assert.True(result.IsError);
        Assert.Equal("Media.NotFound", result.FirstError.Code);
    }

    [DatabaseFact]
    public async Task GetLinks_ReadyMedia_ReturnsSignedVariants()
    {
        var ownerId = Guid.NewGuid();
        var mediaId = await CreateMediaAsync(ownerId, ready: true);

        var links = await LinksAsync(ownerId, [mediaId]);

        var file = Assert.Contains(mediaId, (IDictionary<Guid, MediaFileLinks>)links);
        Assert.Equal(2, file.Variants.Count);
        Assert.True(file.Variants.ContainsKey("thumb"));
        Assert.True(file.Variants.ContainsKey("display"));
        Assert.False(string.IsNullOrWhiteSpace(file.Variants["thumb"].Url));
        Assert.Equal(200, file.Variants["thumb"].Width);
    }

    [DatabaseFact]
    public async Task GetLinks_ForeignAndPending_Omitted()
    {
        var ownerId = Guid.NewGuid();
        var foreign = await CreateMediaAsync(Guid.NewGuid(), ready: true);
        var pending = await CreateMediaAsync(ownerId, ready: false);

        var links = await LinksAsync(ownerId, [foreign, pending]);

        Assert.Empty(links);
    }

    private async Task<ErrorOr<Success>> ReadinessAsync(Guid ownerId, Guid[] mediaIds)
    {
        using var scope = _factory.Services.CreateScope();
        var readiness = scope.ServiceProvider.GetRequiredService<IMediaReadiness>();
        return await readiness.EnsureReadyAsync(ownerId, mediaIds, CancellationToken.None);
    }

    private async Task<IReadOnlyDictionary<Guid, MediaFileLinks>> LinksAsync(Guid ownerId, Guid[] mediaIds)
    {
        using var scope = _factory.Services.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IMediaLinksQuery>();
        return await query.GetLinksAsync(ownerId, mediaIds, CancellationToken.None);
    }

    private async Task<Guid> CreateMediaAsync(Guid ownerId, bool ready)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var now = DateTime.UtcNow;

        var created = MediaObject.Create(ownerId, MediaContentTypes.Png, now);
        Assert.False(created.IsError);
        var media = created.Value;

        if (ready)
        {
            var variants = new[]
            {
                MediaVariant.Create(media.Id, ownerId, MediaVariantName.Thumb, 200, 150, 700, now).Value,
                MediaVariant.Create(media.Id, ownerId, MediaVariantName.Display, 800, 600, 4_000, now).Value,
            };

            var marked = media.MarkReady(MediaFormat.Png, 800, 600, 12_000, variants, now);
            Assert.False(marked.IsError);
        }

        context.Add(media);
        await context.SaveChangesAsync();
        return media.Id;
    }
}
