using Media.Application.Abstractions;
using Media.Application.Storage;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Contracts;

/// <summary>
/// Подписывает ссылки на готовые варианты файлов владельца.
/// Чужие, несуществующие и неготовые файлы в результат не попадают - вызывающий показывает заглушку.
/// </summary>
internal sealed class MediaLinkReader(MediaDbContext context, IObjectStorage storage) : IMediaLinkReader
{
    public async Task<IReadOnlyDictionary<Guid, MediaFileLinks>> ReadAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken)
    {
        if (mediaIds.Count == 0)
            return new Dictionary<Guid, MediaFileLinks>();

        var distinct = mediaIds.Distinct().ToArray();

        var files = await context.MediaObjects
            .AsNoTracking()
            .Where(media => distinct.Contains(media.Id)
                            && media.OwnerId == ownerId
                            && media.Status == MediaStatus.Ready)
            .Include(media => media.Variants)
            .ToArrayAsync(cancellationToken);

        var links = new Dictionary<Guid, MediaFileLinks>(files.Length);

        foreach (var media in files)
        {
            var variants = new Dictionary<string, MediaImageLink>(media.Variants.Count);

            foreach (var variant in media.Variants)
            {
                var url = await storage.CreateReadUrlAsync(
                    variant.ObjectKey.Value, MediaConstraints.ReadLinkLifetime, cancellationToken);
                if (url.IsError)
                    continue;

                variants[variant.Variant.ToStorageName()] = new MediaImageLink(url.Value, variant.Width, variant.Height);
            }

            if (variants.Count > 0)
                links[media.Id] = new MediaFileLinks(variants);
        }

        return links;
    }
}
