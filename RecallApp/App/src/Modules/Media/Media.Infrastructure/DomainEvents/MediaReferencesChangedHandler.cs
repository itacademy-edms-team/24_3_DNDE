using Common.Application.DomainEvents;
using ErrorOr;
using MediatR;
using Media.Application.References;
using Media.Contracts;

namespace Media.Infrastructure.DomainEvents;

internal sealed class MediaReferencesChangedHandler(ISender sender) : IDomainEventHandler<MediaReferencesChanged>
{
    public Task<ErrorOr<Success>> HandleAsync(MediaReferencesChanged domainEvent, CancellationToken cancellationToken) =>
        sender.Send(
            new SyncReferencesCommand(domainEvent.OwnerId, domainEvent.ElementScope, domainEvent.Slots),
            cancellationToken);
}
