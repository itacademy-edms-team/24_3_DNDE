namespace Common.Application.DomainEvents;

/// <summary>
/// Сущность, способная держать доменные события до их материализации в outbox (ADR 0040).
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
