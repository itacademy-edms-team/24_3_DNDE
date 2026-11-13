using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Application.Abstractions;
using Media.Application.Storage;
using Media.Domain.Objects;

namespace Media.Application.Objects;

public sealed record GetMediaQuery(Guid MediaId) : IRequest<ErrorOr<MediaResponse>>;

/// <summary>
/// Возвращает состояние своего файла и его готовые варианты.
/// Заодно лениво проверяет `pending`: если объекта в хранилище нет, файл сразу помечается `failed`,
/// а клиент не ждёт фоновой чистки.
/// </summary>
public sealed class GetMediaHandler(
    IMediaObjectRepository repository,
    IObjectStorage storage,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<GetMediaQuery, ErrorOr<MediaResponse>>
{
    public async Task<ErrorOr<MediaResponse>> Handle(GetMediaQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return MediaErrors.UserUnresolved;

        var media = await repository.FindAsync(query.MediaId, cancellationToken);
        if (media is null || media.OwnerId != ownerId)
            return MediaErrors.NotFound;

        if (media.Status == MediaStatus.Pending)
            await CheckPendingAsync(media, cancellationToken);

        return MediaResponse.From(media);
    }

    /// <summary>
    /// Ленивая проверка: загрузка могла не состояться, а окно доверия ещё не истекло -
    /// значит именно сейчас есть смысл спросить хранилище, а не ждать сутки.
    /// </summary>
    private async Task CheckPendingAsync(MediaObject media, CancellationToken cancellationToken)
    {
        var stored = await storage.HeadAsync(ObjectKey.Upload(media.Id).Value, cancellationToken);
        if (stored is not null)
            return;

        var failed = media.MarkFailed(MediaErrors.ObjectMissing, clock.GetUtcNow().UtcDateTime);
        if (failed.IsError)
            return;

        await repository.SaveChangesAsync(cancellationToken);
    }
}
