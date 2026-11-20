using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Предоставляет правила порядка элементов набора.
/// </summary>
/// <remarks>
/// Порядок задаётся рангами. Новый элемент получает ранг между соседями,
/// остальные при этом не меняются.
/// Правило решает, куда именно попадает элемент,
/// а вычисление значения ранга делегируется <see cref="IPositionGenerator"/>.
/// </remarks>
public static class SetOrdering
{
    /// <summary>
    /// Вычисляет позицию для перемещаемого элемента по его соседям.
    /// </summary>
    /// <remarks>
    /// Существование перемещаемого элемента проверяет вызывающий код: 
    /// здесь проверяются только соседи.
    /// Поэтому метод не возвращает <see cref="ElementErrors.NotFound"/>.
    /// </remarks>
    /// <param name="generator">Вычислитель рангов.</param>
    /// <param name="current">Активные элементы набора по порядку.</param>
    /// <param name="elementId">Перемещаемый элемент.</param>
    /// <param name="previousId">Сосед перед элементом; <see langword="null"/> - начало набора.</param>
    /// <param name="nextId">Сосед после элемента; <see langword="null"/> - конец набора.</param>
    /// <returns>
    /// Позиция для элемента либо ошибка:
    /// <see cref="ElementErrors.OrderNeighboursRequired"/> (не задан ни один сосед),
    /// <see cref="ElementErrors.OrderInvalid"/> (некорректная пара соседей),
    /// <see cref="ElementErrors.OrderNeighbourNotFound"/> (сосед не найден среди активных элементов).
    /// </returns>
    public static ErrorOr<Position> PositionFor(
        IPositionGenerator generator,
        IReadOnlyList<Element> current,
        Guid elementId,
        Guid? previousId,
        Guid? nextId)
    {
        if (previousId is null && nextId is null)
            return ElementErrors.OrderNeighboursRequired;

        if (previousId == nextId)
            return ElementErrors.OrderInvalid("neighbours are the same element");

        if (elementId == previousId || elementId == nextId)
            return ElementErrors.OrderInvalid("element cannot be its own neighbour");

        var previous = previousId is { } previousValue
            ? current.FirstOrDefault(element => element.Id == previousValue)
            : null;

        var next = nextId is { } nextValue
            ? current.FirstOrDefault(element => element.Id == nextValue)
            : null;

        if (previousId is not null && previous is null)
            return ElementErrors.OrderNeighbourNotFound(previousId.Value);

        if (nextId is not null && next is null)
            return ElementErrors.OrderNeighbourNotFound(nextId.Value);

        if (previous is not null && next is not null && string.CompareOrdinal(previous.Position, next.Position) >= 0)
            return ElementErrors.OrderInvalid("previous element is not before the next element");

        return generator.Between(
            previous is null ? null : Position.FromStorage(previous.Position),
            next is null ? null : Position.FromStorage(next.Position));
    }

    /// <summary>
    /// Вычисляет позицию для нового элемента в конце набора.
    /// </summary>
    /// <param name="generator">Вычислитель рангов.</param>
    /// <param name="last">Элемент, после которого встаёт новый; должен быть последним по порядку
    /// (например, из <c>GetLastAsync</c>); <see langword="null"/>, если набор пуст.</param>
    /// <returns>Позиция после последнего элемента.</returns>
    public static Position PositionAtEnd(IPositionGenerator generator, Element? last) =>
        generator.Between(last is null ? null : Position.FromStorage(last.Position), null);
}
