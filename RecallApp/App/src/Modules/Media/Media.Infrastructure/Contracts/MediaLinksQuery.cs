using Media.Application.Abstractions;
using Media.Contracts;

namespace Media.Infrastructure.Contracts;

/// <summary>
/// Ссылки на файлы для других модулей: тот же читатель, что и у HTTP-эндпоинта,
/// чтобы модули и клиент видели одинаковые адреса и одинаковые правила отбора.
/// </summary>
internal sealed class MediaLinksQuery(IMediaLinkReader reader) : IMediaLinksQuery
{
    public Task<IReadOnlyDictionary<Guid, MediaFileLinks>> GetLinksAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken) =>
        reader.ReadAsync(ownerId, mediaIds, cancellationToken);
}
