namespace Common.Domain.DomainEvents;

/// <summary>
/// Доменное событие: факт, который сущность регистрирует в памяти, а outbox доставляет позже.
/// Не зависит от MediatR - связь с ним наводит только инфраструктура (ADR 0040).
/// </summary>
public interface IDomainEvent
{
    /// <summary>Сквозная идентичность события: едет с ним через outbox, брокеры и потребителей.</summary>
    Guid EventId { get; }

    /// <summary>Когда событие произошло.</summary>
    DateTime DateOccurred { get; }

    /// <summary>Стабильное имя события для маршрутизации в outbox (не AssemblyQualifiedName).</summary>
    string EventType { get; }
}
