namespace Common.Domain.DomainEvents;

/// <summary>
/// Определяет сущность,
/// способную накапливать доменные события до их материализации в outbox (ADR 0040).
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>Возвращает зарегистрированные доменные события.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Очищает список зарегистрированных событий.</summary>
    void ClearDomainEvents();
}
