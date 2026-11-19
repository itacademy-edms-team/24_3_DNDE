using Media.Application.Storage;
using Media.Domain.Objects;
using Media.Domain.References;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data.Cleanup;

/// <summary>
/// Чистильщик сирот модуля Media: брошенные загрузки, готовые файлы без ссылок и надгробия удалённых записей.
/// Каждая категория обрабатывается отдельно, чтобы сбой на одной не задерживал остальные (онтология Media §6.1, §7.3).
/// </summary>
internal sealed class MediaCleaner(MediaDbContext context, IObjectStorage storage, TimeProvider clock)
{
    /// <summary>Удаляет `pending`/`failed` старше срока вместе с временными объектами в хранилище.</summary>
    public async Task<int> PurgeAbandonedAsync(DateTime cutoff, int batchSize, CancellationToken cancellationToken)
    {
        var abandoned = await context.MediaObjects
            .AsNoTracking()
            .Where(media => (media.Status == MediaStatus.Pending || media.Status == MediaStatus.Failed)
                            && media.CreatedAt < cutoff)
            .OrderBy(media => media.CreatedAt)
            .Take(batchSize)
            .Select(media => media.Id)
            .ToArrayAsync(cancellationToken);

        if (abandoned.Length == 0)
            return 0;

        foreach (var mediaId in abandoned)
        {
            // Объект мог не долететь: удаление идемпотентно, ошибку не считаем поводом оставить строку.
            await storage.DeleteAsync(ObjectKey.Upload(mediaId).Value, cancellationToken);
        }

        return await context.MediaObjects
            .Where(media => abandoned.Contains(media.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>Помечает `deleted` готовые файлы без ссылок старше срока и убирает их варианты из хранилища.</summary>
    public async Task<int> PurgeUnreferencedAsync(DateTime cutoff, int batchSize, CancellationToken cancellationToken)
    {
        var orphans = await context.MediaObjects
            .Where(media => media.Status == MediaStatus.Ready
                            && media.ReadyAt != null
                            && media.ReadyAt < cutoff
                            && !context.Set<MediaReference>().Any(reference => reference.MediaId == media.Id))
            .OrderBy(media => media.ReadyAt)
            .Take(batchSize)
            .Include(media => media.Variants)
            .ToArrayAsync(cancellationToken);

        if (orphans.Length == 0)
            return 0;

        var now = clock.GetUtcNow().UtcDateTime;

        foreach (var media in orphans)
        {
            media.MarkDeleted(now);

            foreach (var variant in media.Variants)
                await storage.DeleteAsync(variant.ObjectKey.Value, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        return orphans.Length;
    }

    /// <summary>
    /// Убирает надгробия: удаляет их варианты из хранилища и только потом строку из БД.
    /// </summary>
    /// <remarks>
    /// Строка удаляется только после того, как все варианты ушли из хранилища: при сбое S3
    /// надгробие остаётся, и следующий цикл повторяет попытку. Так ни один объект не остаётся
    /// в хранилище без следа.
    /// </remarks>
    public async Task<int> PurgeDeletedAsync(DateTime cutoff, int batchSize, CancellationToken cancellationToken)
    {
        var tombstones = await context.MediaObjects
            .AsNoTracking()
            .Where(media => media.Status == MediaStatus.Deleted
                            && media.DeletedAt != null
                            && media.DeletedAt < cutoff)
            .OrderBy(media => media.DeletedAt)
            .Take(batchSize)
            .Include(media => media.Variants)
            .ToArrayAsync(cancellationToken);

        if (tombstones.Length == 0)
            return 0;

        var removable = new List<Guid>(tombstones.Length);

        foreach (var media in tombstones)
        {
            var allRemoved = true;

            foreach (var variant in media.Variants)
            {
                var result = await storage.DeleteAsync(variant.ObjectKey.Value, cancellationToken);
                if (result.IsError)
                {
                    // Объект остался в хранилище: надгробие сохраняем, чтобы повторить в следующем цикле.
                    allRemoved = false;
                    break;
                }
            }

            if (allRemoved)
                removable.Add(media.Id);
        }

        if (removable.Count == 0)
            return 0;

        return await context.MediaObjects
            .Where(media => removable.Contains(media.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
