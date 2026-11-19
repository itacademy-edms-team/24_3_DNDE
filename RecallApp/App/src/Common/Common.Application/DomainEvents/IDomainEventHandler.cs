using Common.Domain.DomainEvents;
using ErrorOr;

namespace Common.Application.DomainEvents;

/// <summary>
/// Определяет обработчик доменного события.
/// </summary>
/// <remarks>
/// Обязан быть идемпотентным: outbox доставляет at-least-once,
/// поэтому одно событие может прийти несколько раз (ADR 0040).
/// </remarks>
/// <typeparam name="TEvent">Тип события.</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Обрабатывает доменное событие.
    /// </summary>
    /// <param name="domainEvent">Событие.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Успех либо ошибка обработки.</returns>
    Task<ErrorOr<Success>> HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
