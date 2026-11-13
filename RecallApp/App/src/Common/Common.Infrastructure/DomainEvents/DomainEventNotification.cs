using Common.Application.DomainEvents;
using MediatR;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Конверт, делающий чистое доменное событие видимым для MediatR: без него
/// <see cref="IDomainEvent"/>, не наследующий <see cref="INotification"/>, опубликовать нельзя.
/// </summary>
internal sealed class DomainEventNotification<TEvent> : INotification
    where TEvent : IDomainEvent
{
    public DomainEventNotification(TEvent domainEvent) => Event = domainEvent;

    public TEvent Event { get; }
}
