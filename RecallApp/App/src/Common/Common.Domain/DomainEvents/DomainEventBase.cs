namespace Common.Domain.DomainEvents;

/// <summary>
/// База доменного события: заполняет сквозной <see cref="IDomainEvent.EventId"/> и момент
/// <see cref="IDomainEvent.DateOccurred"/>, оставляя конкретному событию только стабильное имя
/// <see cref="IDomainEvent.EventType"/>.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>Возвращает момент возникновения события в UTC.</summary>
    public DateTime DateOccurred { get; init; } = DateTime.UtcNow;

    public abstract string EventType { get; }
}
