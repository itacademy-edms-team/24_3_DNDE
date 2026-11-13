using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Domain.DomainEvents;

/// <summary>
/// Базовая реализация <see cref="IHasDomainEvents"/>: события лежат в памяти и не маппятся в БД.
/// <para>
/// <see cref="RegisterDomainEvent"/> публичный (в отличие от Ardalis.SharedKernel): событие регистрирует
/// application-хендлер, потому что часть данных (например, владелец) известна только ему, а не сущности.
/// В outbox событие превращает SaveChangesInterceptor, а не сама сущность.
/// </para>
/// </summary>
public abstract class HasDomainEventsBase : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Регистрирует событие на отложенную публикацию: в outbox его превратит interceptor при SaveChanges.</summary>
    public void RegisterDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
