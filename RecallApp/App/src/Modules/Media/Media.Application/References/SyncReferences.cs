using ErrorOr;
using MediatR;
using Media.Application.Abstractions;
using Media.Contracts;
using Media.Domain.Objects;
using Media.Domain.References;

namespace Media.Application.References;

/// <summary>
/// Синхронизирует ссылки области с присланным набором слотов.
/// </summary>
/// <remarks>
/// <para>Недостающие слоты снимаются, новые вставляются, изменённые перезаписываются на новые файлы.
/// Набор декларативный, поэтому повтор доставки безопасен.</para>
/// <para>Все файлы проверяются до записи: привязка не применяется частично, а чужой или неготовый
/// файл отклоняет команду целиком.</para>
/// </remarks>
/// <param name="OwnerId">Владелец файлов: чужие файлы привязывать нельзя.</param>
/// <param name="ElementScope">Область ссылок (<c>set:{setId}:element:{elementId}</c>);
/// присланные слоты обязаны лежать внутри неё.</param>
/// <param name="Slots">Полный текущий набор слотов области; всё, чего здесь нет, снимается.</param>
public sealed record SyncReferencesCommand(
    Guid OwnerId,
    string ElementScope,
    IReadOnlyList<MediaReferenceSlot> Slots)
    : IRequest<ErrorOr<Success>>;

public sealed class SyncReferencesHandler(
    IMediaObjectRepository objects,
    IMediaReferenceRepository references,
    TimeProvider clock)
    : IRequestHandler<SyncReferencesCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(SyncReferencesCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ElementScope))
            return MediaErrors.ReferenceInvalid;

        var slots = (command.Slots ?? []).ToArray();

        // Слот обязан принадлежать области: иначе одна карточка могла бы снять чужие ссылки.
        if (slots.Any(slot => !MediaReference.IsInScope(slot.Reference, command.ElementScope)))
            return MediaErrors.ReferenceInvalid;

        if (slots.Select(slot => slot.Reference).Distinct(StringComparer.Ordinal).Count() != slots.Length)
            return MediaErrors.ReferenceInvalid;

        // Проверяем все файлы до записи: привязка не должна применяться частично.
        foreach (var mediaId in slots.Select(slot => slot.MediaId).Distinct())
        {
            var media = await objects.FindAsync(mediaId, cancellationToken);
            if (media is null || media.OwnerId != command.OwnerId)
                return MediaErrors.NotFound;

            if (media.Status != MediaStatus.Ready)
                return MediaErrors.NotReady(slots.First(slot => slot.MediaId == mediaId).Reference);
        }

        await references.SyncAsync(
            command.ElementScope,
            slots,
            clock.GetUtcNow().UtcDateTime,
            cancellationToken);

        return Result.Success;
    }
}
