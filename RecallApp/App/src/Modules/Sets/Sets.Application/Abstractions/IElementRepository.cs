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
    /// Все активные элементы набора по порядку. Нужен, чтобы найти соседей при перестановке
    /// и последний элемент при создании - правила в <see cref="SetOrdering"/>.
    /// </summary>
    Task<IReadOnlyList<Element>> ListForOrderingAsync(Guid setId, CancellationToken cancellationToken);

    /// <summary>Последний по порядку активный элемент набора или <see langword="null"/>, если набор пуст.</summary>
    Task<Element?> GetLastAsync(Guid setId, CancellationToken cancellationToken);

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

    /// <summary>Мягко удаляет все элементы набора при удалении самого набора (онтология §6.6).</summary>
    Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
