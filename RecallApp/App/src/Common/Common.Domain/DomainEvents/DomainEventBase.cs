namespace Common.Domain.DomainEvents;

/// <summary>
/// Предоставляет базовый класс для доменных событий.
/// </summary>
/// <remarks>
/// Заполняет сквозную идентичность <see cref="IDomainEvent.EventId"/> 
/// и момент <see cref="IDomainEvent.DateOccurred"/>,
/// оставляя конкретному событию только стабильное имя <see cref="IDomainEvent.EventType"/>.
/// </remarks>
public abstract record DomainEventBase : IDomainEvent
{
    /// <summary>Возвращает идентификатор события.</summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>Возвращает момент возникновения события в UTC.</summary>
    public DateTime DateOccurred { get; init; } = DateTime.UtcNow;

    /// <summary>Возвращает стабильное имя типа события.</summary>
    public abstract string EventType { get; }
}
