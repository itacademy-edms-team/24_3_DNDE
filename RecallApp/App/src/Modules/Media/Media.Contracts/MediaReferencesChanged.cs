using Common.Application.DomainEvents;

namespace Media.Contracts;

/// <summary>
/// Доменное событие: потребитель (Sets) сообщает, какой файл стоит в каком слоте элемента.
/// Декларативный снимок состояния, поэтому повторная доставка безопасна (онтология Media §6.4).
/// </summary>
/// <param name="OwnerId">Владелец файлов.</param>
/// <param name="ElementScope">Область элемента (`set:{setId}:element:{elementId}`) или набора (`set:{setId}`).</param>
/// <param name="Slots">Полный набор слотов; пустой список означает «отвязать всё».</param>
public sealed record MediaReferencesChanged(
    Guid OwnerId,
    string ElementScope,
    IReadOnlyList<MediaReferenceSlot> Slots) : DomainEventBase
{
    public override string EventType => MediaOutboxMessages.ReferencesSync;
}
