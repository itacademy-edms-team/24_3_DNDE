using Common.Domain.DomainEvents;

namespace Media.Contracts;

/// <summary>
/// Представляет доменное событие: Sets сообщает, какой файл стоит в каком слоте элемента.
/// </summary>
/// <remarks>
/// Событие несёт полный снимок слотов области, а не дельту изменений,
/// поэтому повторная доставка безопасна:
/// применение сходится к одному состоянию (онтология Media §6.4).
/// </remarks>
/// <param name="OwnerId">Владелец файлов.</param>
/// <param name="ElementScope">Область элемента (<c>set:{setId}:element:{elementId}</c>) или набора
/// (<c>set:{setId}</c>).</param>
/// <param name="Slots">Полный набор слотов; пустой список означает «отвязать всё».</param>
public sealed record MediaReferencesChanged(
    Guid OwnerId,
    string ElementScope,
    IReadOnlyList<MediaReferenceSlot> Slots) : DomainEventBase
{
    public override string EventType => MediaOutboxMessages.ReferencesSync;
}
