using Common.Domain.DomainEvents;
using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Представляет элемент набора - единицу учебного контента.
/// Поля и способ проверки определяются типом элемента.
/// </summary>
/// <remarks>
/// <para>Элемент всегда принадлежит одному набору и без него не существует.</para>
/// <para>Позиция - строковый ранг, уникальный среди активных элементов набора:
/// перестановка меняет только перемещаемый элемент, остальные остаются на месте.</para>
/// <para>Чужие элементы неотличимы от несуществующих:
/// операции с ними завершаются ошибкой <see cref="ElementErrors.NotFound"/>.</para>
/// <para>Жизненный цикл: активен -> помечен на удаление -> удалён физически.
/// Пока элемент помечен, но ещё не удалён, он недоступен для чтения и правки.</para>
/// <para><see cref="UpdatedAt"/> обновляется при изменении содержимого за исключением смены позиции:
/// перестановка считается изменением набора, и метка двигается у набора, а не у элемента.</para>
/// </remarks>
public sealed class Element : HasDomainEventsBase
{
    private Element() { }

    /// <summary>
    /// Возвращает идентификатор элемента.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Возвращает идентификатор набора, которому принадлежит элемент.
    /// </summary>
    public Guid SetId { get; private set; }

    /// <summary>
    /// Возвращает позицию элемента в наборе.
    /// </summary>
    /// <remarks>
    /// Меняется только через <see cref="MoveTo"/> по соседям, вычисленным правилом порядка:
    /// остальные элементы набора при этом не затрагиваются.
    /// Строка (а не <see cref="Position"/>) - чтобы БД-запросы могли
    /// сравнивать и сортировать столбец напрямую.
    /// </remarks>
    public string Position { get; private set; } = string.Empty;
    
    /// <summary>
    /// Возвращает тип элемента.
    /// </summary>
    /// <remarks>
    /// Выводится из payload и после создания не меняется:
    /// <see cref="ReplaceContent"/> допускает только тот же тип на замену.
    /// </remarks>
    public ElementType Type { get; private set; }

    /// <summary>
    /// Возвращает содержимое элемента; конкретный тип задаёт <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// Значение никогда не <see langword="null"/>: оно задаётся при создании элемента
    /// и восстанавливается при загрузке из хранилища.
    /// </remarks>
    public ElementPayload Payload { get; private set; } = null!;

    /// <summary>
    /// Возвращает момент создания элемента в UTC.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент последнего изменения элемента в UTC.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент мягкого удаления элемента в UTC или <see langword="null"/>, если элемент не удалён.
    /// </summary>
    /// <remarks>
    /// Пометка идемпотентна: нельзя изменить состояние уже установленной пометки.
    /// Физически строку удаляет фоновая чистка по истечении срока хранения.
    /// </remarks>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Создаёт элемент набора на указанной позиции.
    /// </summary>
    /// <remarks>
    /// Проверяется только неотрицательность позиции: попадание в диапазон набора и занятость
    /// позиции обеспечивает вызывающий код, пересчитывающий порядок.
    /// </remarks>
    /// <param name="setId">Идентификатор набора.</param>
    /// <param name="payload">Содержимое элемента; тип элемента выводится из payload.</param>
    /// <param name="position">Позиция в наборе; нумерация начинается с нуля.</param>
    /// <param name="utcNow">Момент создания в UTC. Задаётся вызывающим кодом.</param>
    /// <returns>
    /// Новый элемент на указанной позиции или ошибка:
    /// <see cref="ElementErrors.SetRequired"/>, <see cref="ElementErrors.PayloadRequired"/>.
    /// </returns>
    public static ErrorOr<Element> Create(Guid setId, ElementPayload payload, Position position, DateTime utcNow)
    {
        if (setId == Guid.Empty)
            return ElementErrors.SetRequired;

        if (payload is null)
            return ElementErrors.PayloadRequired;

        return new Element
        {
            Id = Guid.NewGuid(),
            SetId = setId,
            Type = payload.Type,
            Position = position.Value,
            Payload = payload,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };
    }

    /// <summary>
    /// Заменяет содержимое элемента целиком.
    /// </summary>
    /// <remarks>
    /// Если payload равен текущему, элемент не меняется и <see cref="UpdatedAt"/> не обновляется.
    /// </remarks>
    /// <param name="payload">Новое содержимое; тип должен совпадать с <see cref="Type"/>.</param>
    /// <param name="utcNow">Момент изменения элемента в UTC. Задаётся вызывающим кодом.</param>
    /// <returns>
    /// <see cref="ErrorOr.Updated"/>, если содержимое принято (в том числе когда оно не изменилось),
    /// иначе ошибка: <see cref="ElementErrors.PayloadRequired"/> или <see cref="ElementErrors.TypeMismatch"/>.
    /// </returns>
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
    /// Помечает набор как удалённый.
    /// </summary>
    /// <remarks>
    /// При первой пометке обновляет <see cref="UpdatedAt"/>.
    /// </remarks>
    /// <param name="utcNow">Текущий момент в UTC, который станет меткой. Задаётся вызывающим кодом.</param>
    public void MarkDeleted(DateTime utcNow)
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = utcNow;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Задаёт новую позицию элемента.
    /// </summary>
    /// <remarks>
    /// Обновляет только позицию: остальные элементы набора и <see cref="UpdatedAt"/> не меняются,
    /// потому что перестановка считается изменением набора, а не элемента.
    /// </remarks>
    /// <param name="newPosition">Новый ранг между соседями, вычисленный правилом порядка.</param>
    public void MoveTo(Position newPosition)
    {
        Position = newPosition.Value;
    }
}
