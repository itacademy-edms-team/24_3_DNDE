namespace Common.Application.DomainEvents;

/// <summary>
/// Обработчик доменного события.
/// Обязан быть идемпотентным: outbox доставляет at-least-once,
/// поэтому одно событие может прийти несколько раз (ADR 0040).
/// </summary>
/// <typeparam name="TEvent">Тип события.</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
