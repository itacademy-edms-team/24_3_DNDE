using Media.Application.Abstractions;
using Media.Contracts;

namespace Media.Infrastructure.Contracts;

/// <summary>
/// Реализует чтение подписанных ссылок для других модулей.
/// </summary>
/// <remarks>
/// Делегирует общему читателю, чтобы модули и HTTP-эндпоинт отдавали одинаковые адреса
/// и применяли одинаковые правила отбора.
/// </remarks>
internal sealed class MediaLinksQuery(IMediaLinkReader reader) : IMediaLinksQuery
{
    public Task<IReadOnlyDictionary<Guid, MediaFileLinks>> GetLinksAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken) =>
        reader.ReadAsync(ownerId, mediaIds, cancellationToken);
}
