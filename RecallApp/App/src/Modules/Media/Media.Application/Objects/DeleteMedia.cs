using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Application.Abstractions;
using Media.Application.Storage;
using Media.Domain.Objects;

namespace Media.Application.Objects;

/// <summary>
/// Удаляет файл владельца.
/// </summary>
/// <remarks>
/// <para>Пока на файл ссылается хотя бы один элемент, удаление запрещено (онтология Media §6.4).</para>
/// <para>После удаления запись становится надгробием, варианты убираются из хранилища сразу.</para>
/// <para>Повторное удаление уже удалённого файла возвращает успех: операция идемпотентна.</para>
/// <para>Проверка ссылок и переход в <c>deleted</c> выполняются
/// в одной транзакции под блокировкой строки,
/// чтобы параллельная синхронизация ссылок не привязала файл между проверкой и удалением.</para>
/// </remarks>
/// <param name="MediaId">Идентификатор файла, который нужно удалить.</param>
public sealed record DeleteMediaCommand(Guid MediaId) : IRequest<ErrorOr<Deleted>>;

public sealed class DeleteMediaHandler(
    IMediaObjectRepository repository,
    IObjectStorage storage,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<DeleteMediaCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteMediaCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return MediaErrors.UserUnresolved;

        var media = await repository.FindAsync(command.MediaId, cancellationToken);
        if (media is null || media.OwnerId != ownerId)
            return MediaErrors.NotFound;

        if (media.Status == MediaStatus.Deleted)
            return Result.Deleted;

        // Проверка ссылок и переход в `deleted` - одна транзакция под блокировкой строки:
        // параллельная синхронизация ссылок не должна успеть привязать файл между проверкой и удалением.
        var outcome = await repository.ExecuteDeletionAsync<ErrorOr<Deleted>>(
            media,
            async token =>
            {
                var references = await repository.CountReferencesAsync(media.Id, token);
                if (references > 0)
                    return MediaErrors.InUse;

                var deleted = media.MarkDeleted(clock.GetUtcNow().UtcDateTime);
                if (deleted.IsError)
                    return deleted.Errors;

                await repository.SaveChangesAsync(token);
                return Result.Deleted;
            },
            cancellationToken);

        if (outcome.IsError)
            return outcome;

        // Объекты удаляем после коммита: сбой хранилища оставит мусор, но не сломает состояние записи.
        foreach (var variant in media.Variants)
            await storage.DeleteAsync(variant.ObjectKey.Value, cancellationToken);

        return outcome;
    }
}
