using ErrorOr;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Contracts;

/// <summary>
/// Реализует проверку готовности файлов для других модулей.
/// </summary>
/// <remarks>
/// Отвечает на вопрос "можно ли привязывать эти файлы" до того,
/// как ссылка попадёт в данные другого модуля.
/// </remarks>
internal sealed class MediaReadiness(MediaDbContext context) : IMediaReadiness
{
    public async Task<ErrorOr<Success>> EnsureReadyAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken)
    {
        if (mediaIds.Count == 0)
            return Result.Success;

        var distinct = mediaIds.Distinct().ToArray();

        var found = await context.MediaObjects
            .AsNoTracking()
            .Where(media => distinct.Contains(media.Id))
            .Select(media => new { media.Id, media.OwnerId, media.Status })
            .ToArrayAsync(cancellationToken);

        foreach (var mediaId in distinct)
        {
            var media = found.SingleOrDefault(row => row.Id == mediaId);
            if (media is null || media.OwnerId != ownerId)
                return MediaErrors.NotFound;

            if (media.Status != MediaStatus.Ready)
                return MediaErrors.NotReady(mediaId.ToString("D"));
        }

        return Result.Success;
    }
}
