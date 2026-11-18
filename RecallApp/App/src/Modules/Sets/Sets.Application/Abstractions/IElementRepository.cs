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

    /// <summary>
    /// Все активные элементы набора по порядку (0, 1, 2, ...). Нужен, чтобы посчитать новые
    /// позиции при вставке, удалении или перестановке - это делают правила в <see cref="SetOrdering"/>.
    /// Объекты возвращаются без отслеживания изменений: позиции записывает отдельный вызов
    /// <see cref="ApplyPositionsAsync"/> - одним SQL с временным сдвигом, иначе уникальный индекс
    /// не даст поменять позиции местами.
    /// </summary>
    Task<IReadOnlyList<Element>> ListForOrderingAsync(Guid setId, CancellationToken cancellationToken);

    Task<Element?> FindAsync(Guid setId, Guid elementId, CancellationToken cancellationToken);

    /// <summary>Id активных элементов набора: нужен для проверки полноты присланного порядка.</summary>
    Task<IReadOnlyList<Guid>> ListActiveIdsAsync(Guid setId, CancellationToken cancellationToken);

    /// <summary>Считает число активных элементов набора.</summary>
    Task<int> CountActiveAsync(Guid setId, CancellationToken cancellationToken);

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
    /// Применяет доменные назначения позиций.
    /// Двухфазная запись (временный сдвиг за пределы диапазона, затем целевые значения)
    /// нужна, потому что частичный уникальный индекс не допускает промежуточных коллизий.
    /// </summary>
    /// <remarks>
    /// Единственное место, где позиции попадают в базу: в домене сеттер закрыт.
    /// Что кому достанется, решает домен - здесь значения только применяются.
    /// Загруженные элементы не меняются после вызова и их нельзя использовать.
    /// </remarks>
    Task ApplyPositionsAsync(
        Guid setId,
        IReadOnlyList<ElementPosition> positions,
        CancellationToken cancellationToken);

    /// <summary>Мягко удаляет все элементы набора при удалении самого набора (онтология §6.6).</summary>
    Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
