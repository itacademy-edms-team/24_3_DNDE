using Common.Domain.DomainEvents;
using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Элемент набора.
/// Не может существовать без обязательно привязки к набору.
/// </summary>
public sealed class Element : HasDomainEventsBase
{
    private Element() { }

    public Guid Id { get; private set; }

    public Guid SetId { get; private set; }

    public int Position { get; private set; }
    
    /// <summary>Дискриминатор состава payload; выводится из payload и после создания не меняется.</summary>
    public ElementType Type { get; private set; }

    public ElementPayload Payload { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    /// <summary>Метка мягкого удаления; null - элемент активен.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Создаёт элемент из готового payload.
    /// </summary>
    public static ErrorOr<Element> Create(Guid setId, ElementPayload payload, int position, DateTime utcNow)
    {
        if (setId == Guid.Empty)
            return ElementErrors.SetRequired;

        if (payload is null)
            return ElementErrors.PayloadRequired;

        if (position < 0)
            return ElementErrors.PositionInvalid(position);

        return new Element
        {
            Id = Guid.NewGuid(),
            SetId = setId,
            Type = payload.Type,
            Position = position,
            Payload = payload,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };
    }

    /// <summary>Проводит полную замену содержимого элемента заданного типа; тип элемента не меняется, позиция не меняется.</summary>
    public ErrorOr<Updated> ReplaceContent(ElementPayload payload, DateTime utcNow)
    {
        if (payload is null)
            return ElementErrors.PayloadRequired;

        if (payload.Type != Type)
            return ElementErrors.TypeMismatch(Type, payload.Type);

        if (payload == Payload)
            return Result.Updated;

        Payload = payload;
        UpdatedAt = utcNow;
        return Result.Updated;
    }

    /// <summary>
    /// Помечает элемент на удаление.
    /// Повторная пометка не обновляет временные метки
    /// </summary>
    public void MarkDeleted(DateTime utcNow)
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = utcNow;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Меняет позицию элемента в наборе.
    /// Не трогает <see cref="UpdatedAt"/>.
    /// </summary>
    public void MoveTo(int newPosition)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(newPosition);

        Position = newPosition;
    }
}
