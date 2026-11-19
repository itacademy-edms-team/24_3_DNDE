using Common.Domain.DomainEvents;
using ErrorOr;

namespace Common.Application.DomainEvents;

/// <summary>
/// Определяет отправку доменных событий их обработчикам.
/// </summary>
/// <remarks>
/// Единственный вызывающий - outbox-воркер:
/// события доставляются асинхронно, immediate dispatch не используется (ADR 0040).
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Отправляет события их обработчикам.
    /// </summary>
    /// <param name="events">События для доставки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Успех либо ошибки, которые вернули обработчики событий.</returns>
    Task<ErrorOr<Success>> DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken);
}
