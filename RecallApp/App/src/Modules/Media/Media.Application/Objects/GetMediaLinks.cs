using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Application.Abstractions;
using Media.Contracts;
using Media.Domain.Objects;

namespace Media.Application.Objects;

/// <summary>
/// Ссылки на чтение для набора файлов владельца: нужны, чтобы продлить адреса,
/// не перезагружая сам контент, который их использует.
/// </summary>
/// <param name="MediaIds">Идентификаторы файлов в любом порядке; дубликаты не считаются ошибкой.</param>
public sealed record GetMediaLinksQuery(IReadOnlyCollection<Guid> MediaIds)
    : IRequest<ErrorOr<MediaLinksResponse>>;

/// <summary>Тело запроса на подпись ссылок: пачка идентификаторов со страницы клиента.</summary>
/// <param name="MediaIds">Отсутствующее поле трактуется как пустой список.</param>
public sealed record GetMediaLinksRequest(IReadOnlyCollection<Guid>? MediaIds);

/// <summary>Ответ на подпись ссылок: карта «файл → варианты». Чужих и неготовых файлов в ней нет.</summary>
public sealed record MediaLinksResponse(IReadOnlyDictionary<Guid, MediaFileLinks> Links);

public sealed class GetMediaLinksHandler(IMediaLinkReader links, ICurrentUser currentUser)
    : IRequestHandler<GetMediaLinksQuery, ErrorOr<MediaLinksResponse>>
{
    public async Task<ErrorOr<MediaLinksResponse>> Handle(
        GetMediaLinksQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return MediaErrors.UserUnresolved;

        var distinct = query.MediaIds.Distinct().ToArray();
        if (distinct.Length > MediaConstraints.MaxLinksPerRequest)
            return MediaErrors.LinksLimitExceeded(distinct.Length);

        if (distinct.Length == 0)
            return new MediaLinksResponse(new Dictionary<Guid, MediaFileLinks>());

        return new MediaLinksResponse(await links.ReadAsync(ownerId, distinct, cancellationToken));
    }
}
