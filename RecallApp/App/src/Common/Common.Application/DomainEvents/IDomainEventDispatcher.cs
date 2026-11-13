using ErrorOr;

namespace Common.Application.DomainEvents;

/// <summary>
/// Отправляет доменные события их обработчикам.
/// Единственный вызывающий - outbox-воркер: immediate dispatch не используется, события доставляются асинхронно (ADR 0040).
/// </summary>
public interface IDomainEventDispatcher
{
    Task<ErrorOr<Success>> DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken);
}
