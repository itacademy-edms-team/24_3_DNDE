using Common.Application.DomainEvents;
using MediatR;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Generic обработчик MediatR:
/// для каждого конкретного <typeparamref name="TEvent"/> 
/// раздаёт событие нашим <see cref="IDomainEventHandler{TEvent}"/>.
/// Регистрируется один раз, а не на событие.
/// </summary>
internal sealed class DomainEventNotificationHandler<TEvent>(IEnumerable<IDomainEventHandler<TEvent>> handlers)
    : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent
{
    public async Task Handle(DomainEventNotification<TEvent> notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
            await handler.HandleAsync(notification.Event, cancellationToken);
    }
}
