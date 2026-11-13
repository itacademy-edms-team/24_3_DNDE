using Media.Application.Abstractions;
using Media.Domain.Objects;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data;

/// <summary>
/// EF-реализация хранилища медиа-объектов.
/// Владение проверяется в сценариях, здесь только доступ к данным.
/// </summary>
internal sealed class MediaObjectRepository(MediaDbContext context) : IMediaObjectRepository
{
    public Task<int> CountPendingAsync(Guid ownerId, CancellationToken cancellationToken) =>
        context.MediaObjects.CountAsync(
            media => media.OwnerId == ownerId && media.Status == MediaStatus.Pending,
            cancellationToken);

    public Task<MediaObject?> FindAsync(Guid mediaId, CancellationToken cancellationToken) =>
        context.MediaObjects
            .Include(media => media.Variants)
            .SingleOrDefaultAsync(media => media.Id == mediaId, cancellationToken);

    public void Add(MediaObject media) => context.MediaObjects.Add(media);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
