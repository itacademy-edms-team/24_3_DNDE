using Common.Application.DomainEvents;
using Common.Domain.DomainEvents;
using ErrorOr;
using MediatR;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Open-generic обработчик MediatR: для каждого конкретного <typeparamref name="TEvent"/> раздаёт
/// событие нашим <see cref="IDomainEventHandler{TEvent}"/>. Регистрируется один раз, а не на событие.
/// Ошибку обработчика превращает в <see cref="DomainEventDispatchException"/> — только так её
/// можно протащить через <see cref="IPublisher"/>, который возвращает <see cref="Task"/>, а не ErrorOr.
/// </summary>
internal sealed class DomainEventNotificationHandler<TEvent>(IEnumerable<IDomainEventHandler<TEvent>> handlers)
    : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent
{
    public async Task Handle(DomainEventNotification<TEvent> notification, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        foreach (var handler in handlers)
        {
            var result = await handler.HandleAsync(notification.Event, cancellationToken);
            if (result.IsError)
                errors.AddRange(result.Errors);
        }

        if (errors.Count > 0)
            throw new DomainEventDispatchException(errors);
    }
}
