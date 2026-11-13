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
    /// <param name="after">Курсор предыдущей страницы; null - первая страница.</param>
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

    /// <summary>
    /// Выполняет действие над составом набора в транзакции с блокировкой строки набора
    /// (<c>SELECT … FOR UPDATE</c>). Параметр действия - число активных элементов, посчитанное
    /// под блокировкой: оно не изменится до конца транзакции.
    /// Все мутации состава идут через этот метод, включая проверку инварианта (например,
    /// сверку присланного порядка с текущим составом): проверка вне лока оставляет окно,
    /// в котором состав успевает измениться.
    /// </summary>
    Task<TResult> ExecuteLockedAsync<TResult>(
        Guid setId,
        Func<int, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken);

    void Add(Element element);

    /// <summary>
    /// Сдвигает активные элементы с позиции <paramref name="fromPosition"/> на +1.
    /// Двухфазная запись: частичный уникальный индекс не даёт сдвигать строки «на месте».
    /// </summary>
    Task<int> ShiftPositionsFromAsync(Guid setId, int fromPosition, CancellationToken cancellationToken);

    /// <summary>Мягко удаляет все элементы набора при удалении самого набора (онтология §6.6).</summary>
    Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Мягко удаляет элемент и уплотняет позиции остальных.
    /// Вызывается внутри <see cref="ExecuteLockedAsync"/>: собственную транзакцию не открывает.
    /// Удаление фиксируется до сдвига: частичный уникальный индекс
    /// не пустит соседа на ещё занятую позицию.
    /// </summary>
    /// <returns>false - элемента нет, он удалён или принадлежит другому набору.</returns>
    Task<bool> DeleteAndCompactAsync(Guid setId, Guid elementId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Переставляет элементы в заданном порядке: позиции становятся `0..n-1`.
    /// Вызывается внутри <see cref="ExecuteLockedAsync"/> - состав уже проверен под локом.
    /// Двухфазная запись: частичный уникальный индекс не даёт поменять две строки местами на месте.
    /// </summary>
    /// <returns>Сколько элементов реально сменили позицию; 0 - порядок элементов не поменялся.</returns>
    Task<int> ApplyOrderAsync(
        Guid setId,
        IReadOnlyList<Guid> orderedElementIds,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
