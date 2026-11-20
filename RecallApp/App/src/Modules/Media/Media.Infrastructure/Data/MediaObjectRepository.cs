using ErrorOr;
using Media.Application.Abstractions;
using Media.Domain.Objects;
using Media.Domain.References;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data;

/// <summary>
/// Реализует хранилище медиа-объектов поверх EF Core.
/// </summary>
/// <remarks>
/// Владение проверяется в сценариях, здесь только доступ к данным.
/// </remarks>
internal sealed class MediaObjectRepository(MediaDbContext context) : IMediaObjectRepository
{
    public Task<int> CountPendingAsync(Guid ownerId, DateTime now, CancellationToken cancellationToken) =>
        context.MediaObjects.CountAsync(
            media => media.OwnerId == ownerId
                     && media.Status == MediaStatus.Pending
                     && media.UploadExpiresAt > now,
            cancellationToken);

    public Task<int> MarkExpiredUploadsAsync(Guid ownerId, DateTime now, CancellationToken cancellationToken) =>
        context.MediaObjects
            .Where(media => media.OwnerId == ownerId
                            && media.Status == MediaStatus.Pending
                            && media.UploadExpiresAt <= now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(media => media.Status, MediaStatus.Failed)
                    .SetProperty(media => media.FailureCode, MediaErrors.UploadExpired.Code)
                    .SetProperty(media => media.FailureDescription, MediaErrors.UploadExpired.Description)
                    .SetProperty(media => media.UpdatedAt, now),
                cancellationToken);

    public Task<MediaObject?> FindAsync(Guid mediaId, CancellationToken cancellationToken) =>
        context.MediaObjects
            .Include(media => media.Variants)
            .SingleOrDefaultAsync(media => media.Id == mediaId, cancellationToken);

    public Task<int> CountReferencesAsync(Guid mediaId, CancellationToken cancellationToken) =>
        context.Set<MediaReference>().CountAsync(reference => reference.MediaId == mediaId, cancellationToken);

    public async Task<TResult> ExecuteDeletionAsync<TResult>(
        MediaObject media,
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Блокировка строки файла: синхронизация ссылок, читающая статус, дождётся коммита.
        await context.Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM media.media_objects WHERE id = {0} FOR UPDATE",
            [media.Id],
            cancellationToken);

        var result = await action(cancellationToken);

        if (result is IErrorOr { IsError: true })
            await transaction.RollbackAsync(cancellationToken);
        else
            await transaction.CommitAsync(cancellationToken);

        return result;
    }

    public void Add(MediaObject media) => context.MediaObjects.Add(media);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
