using Common.Application.DomainEvents;
using ErrorOr;
using MediatR;
using Media.Application.References;
using Media.Contracts;

namespace Media.Infrastructure.DomainEvents;

/// <summary>
/// Применяет декларативный набор слотов элемента: переиспользует прикладной сценарий
/// <see cref="SyncReferencesCommand"/>. Идемпотентен, потому что снимок полный.
/// </summary>
internal sealed class MediaReferencesChangedHandler(ISender sender) : IDomainEventHandler<MediaReferencesChanged>
{
    public Task<ErrorOr<Success>> HandleAsync(MediaReferencesChanged domainEvent, CancellationToken cancellationToken) =>
        sender.Send(
            new SyncReferencesCommand(domainEvent.OwnerId, domainEvent.ElementScope, domainEvent.Slots),
            cancellationToken);
}
