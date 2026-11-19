using ErrorOr;
using Media.Application.Abstractions;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Domain.References;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data;

/// <summary>
/// Реализует хранилище ссылок на файлы поверх EF Core.
/// </summary>
/// <remarks>
/// Слот уникален, поэтому замена картинки - это <c>UPDATE</c> одной строки,
/// а снятие слота - удаление строки области (онтология Media §6.4).
/// </remarks>
internal sealed class MediaReferenceRepository(MediaDbContext context) : IMediaReferenceRepository
{
    public async Task SyncAsync(
        string elementScope,
        IReadOnlyCollection<MediaReferenceSlot> slots,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var existing = await context.Set<MediaReference>()
            .Where(reference => reference.Reference == elementScope
                                || reference.Reference.StartsWith(elementScope + ":"))
            .ToArrayAsync(cancellationToken);

        var incoming = slots.ToDictionary(slot => slot.Reference, slot => slot.MediaId, StringComparer.Ordinal);

        // Слоты, которых нет в присланном наборе, снимаются: потребитель присылает полное состояние.
        foreach (var reference in existing.Where(reference => !incoming.ContainsKey(reference.Reference)))
            context.Remove(reference);

        foreach (var (reference, mediaId) in incoming)
        {
            var row = existing.FirstOrDefault(existingReference =>
                string.Equals(existingReference.Reference, reference, StringComparison.Ordinal));

            if (row is null)
            {
                var created = MediaReference.Create(mediaId, reference, now);
                if (created.IsError)
                    throw new InvalidOperationException($"Reference '{reference}' failed validation: {created.FirstError.Code}");

                context.Add(created.Value);
                continue;
            }

            var moved = row.PointTo(mediaId, now);
            if (moved.IsError)
                throw new InvalidOperationException($"Failed to update slot '{reference}': {moved.FirstError.Code}");
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
