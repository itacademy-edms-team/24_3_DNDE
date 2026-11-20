namespace Common.Domain.DomainEvents;

/// <summary>
/// Определяет доменное событие.
/// </summary>
/// <remarks>
/// Факт, который сущность регистрирует в памяти, а outbox доставляет позже.
/// Не зависит от MediatR: связь с ним реализует только инфраструктура (ADR 0040).
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// Возвращает сквозную идентичность события: 
    /// едет с ним через outbox, брокеры и потребителей.
    /// </summary>
    Guid EventId { get; }

    /// <summary>Возвращает момент возникновения события в UTC.</summary>
    DateTime DateOccurred { get; }

    /// <summary>Возвращает стабильное имя события для маршрутизации в outbox
    /// (не AssemblyQualifiedName).</summary>
    string EventType { get; }
}
