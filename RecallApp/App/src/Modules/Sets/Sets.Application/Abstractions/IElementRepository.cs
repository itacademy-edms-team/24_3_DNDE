using Sets.Application.Elements;
using Sets.Domain.Elements;

namespace Sets.Application.Abstractions;

/// <summary>
/// Репозиторий элементов набора в схеме sets.
/// Владение набором проверяет вызывающий код, поэтому выборки ограничены только id набора.
/// </summary>
public interface IElementRepository
{
    /// <summary>
    /// Страница элементов набора в порядке <c>position ASC, id ASC</c>.
    /// </summary>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="after">Курсор предыдущей страницы; null — первая страница.</param>
    /// <param name="take">Сколько строк вернуть (вызывающий берёт на одну больше).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyList<Element>> ListBySetAsync(
        Guid setId,
        ElementCursor? after,
        int take,
        CancellationToken cancellationToken);

    Task<Element?> FindAsync(Guid setId, Guid elementId, CancellationToken cancellationToken);

    /// <summary>Id активных элементов набора: нужен для проверки полноты присланного порядка.</summary>
    Task<IReadOnlyList<Guid>> ListActiveIdsAsync(Guid setId, CancellationToken cancellationToken);

    /// <summary>Позиция в конец набора: <c>MAX(position) + 1</c> для активных элементов.</summary>
    Task<int> NextPositionAsync(Guid setId, CancellationToken cancellationToken);

    void Add(Element element);

    /// <summary>Мягко удаляет все элементы набора при удалении самого набора (онтология §6.6).</summary>
    Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Мягко удаляет элемент и сразу уплотняет позиции остальных в одной транзакции.
    /// Удаление обязано быть зафиксировано до сдвига:
    /// частичный уникальный индекс не пустит соседа на ещё занятую позицию.
    /// </summary>
    /// <returns>false - элемента нет, он удалён или принадлежит другому набору.</returns>
    Task<bool> DeleteAndCompactAsync(Guid setId, Guid elementId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Переставляет элементы в заданном порядке: позиции становятся `0..n-1`.
    /// Запись должна быть двухфазной, потому что частичный уникальный индекс не даёт поменять две строки местами на месте.
    /// </summary>
    Task ApplyOrderAsync(
        Guid setId,
        IReadOnlyList<Guid> orderedElementIds,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
