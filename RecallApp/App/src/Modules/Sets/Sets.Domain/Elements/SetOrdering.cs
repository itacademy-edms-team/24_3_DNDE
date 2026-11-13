using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Правила порядка элементов набора: позиции плотные `0..n-1`, перестановка - только полный список.
/// Домен решает, кто и куда двигается; применение к строкам (без конфликта уникального индекса) -
/// задача инфраструктуры.
/// </summary>
public static class SetOrdering
{
    /// <summary>
    /// Проверяет присланный порядок и возвращает целевые позиции для всех элементов.
    /// Несовпадение состава - 409.
    /// </summary>
    public static ErrorOr<IReadOnlyList<ElementPosition>> Assign(
        IReadOnlyList<Element> current,
        IReadOnlyList<Guid>? requested)
    {
        if (requested is not { Count: > 0 })
            return ElementErrors.OrderRequired;

        var requestedIds = requested.ToHashSet();
        if (requestedIds.Count != requested.Count)
            return ElementErrors.OrderMismatch;

        if (!requestedIds.SetEquals(current.Select(element => element.Id)))
            return ElementErrors.OrderMismatch;

        return requested
            .Select((elementId, position) => new ElementPosition(elementId, position))
            .ToList();
    }

    /// <summary>
    /// Позиции для вставки в <paramref name="position"/>: все элементы с позицией >= неё сдвигаются на +1.
    /// </summary>
    public static ErrorOr<IReadOnlyList<ElementPosition>> ForInsert(
        IReadOnlyList<Element> current,
        int position)
    {
        if (position < 0 || position > current.Count)
            return ElementErrors.PositionOutOfRange(position, current.Count);

        return current
            .Where(element => element.Position >= position)
            .Select(element => new ElementPosition(element.Id, element.Position + 1))
            .ToList();
    }

    /// <summary>
    /// Позиции для уплотнения после удаления: все элементы ниже удалённой позиции сдвигаются на −1.
    /// </summary>
    public static IReadOnlyList<ElementPosition> ForRemoval(
        IReadOnlyList<Element> current,
        int removedPosition) =>
        current
            .Where(element => element.Position > removedPosition)
            .Select(element => new ElementPosition(element.Id, element.Position - 1))
            .ToList();
}
