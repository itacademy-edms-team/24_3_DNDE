using System.Collections.Concurrent;
using Common.Application.DomainEvents;
using ErrorOr;
using MediatR;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Единственное место, знающее про MediatR: оборачивает доменное событие в
/// <see cref="DomainEventNotification{TEvent}"/> и публикует через <see cref="IPublisher"/>.
/// Рефлексия на обёртку кэшируется по типу события; ошибки обработчиков возвращаются как ErrorOr.
/// </summary>
public sealed class MediatRDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Func<IDomainEvent, INotification>> Factories = new();

    public async Task<ErrorOr<Success>> DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in events)
        {
            var notification = Wrap(domainEvent);

            try
            {
                await publisher.Publish(notification, cancellationToken);
            }
            catch (DomainEventDispatchException exception)
            {
                return exception.Errors.ToList();
            }
        }

        return Result.Success;
    }

    private static INotification Wrap(IDomainEvent domainEvent)
    {
        var factory = Factories.GetOrAdd(domainEvent.GetType(), eventType =>
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
            var constructor = notificationType.GetConstructor([eventType])!;
            return domainEventArg => (INotification)constructor.Invoke([domainEventArg]);
        });

        return factory(domainEvent);
    }
}
