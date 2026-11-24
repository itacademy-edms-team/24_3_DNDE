using Sets.Application.Elements;
using Sets.Domain.Elements;

namespace Sets.Application.Abstractions;

/// <summary>
/// Определяет хранилище элементов набора.
/// </summary>
/// <remarks>
/// Владение набором проверяет вызывающий код.
/// Выборки ограничены только идентификатором набора, а не владельцем.
/// </remarks>
public interface IElementRepository
{
    /// <summary>
    /// Возвращает страницу элементов набора в порядке рангов.
    /// </summary>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="after">Курсор предыдущей страницы; <see langword="null"/> - первая страница.</param>
    /// <param name="take">Сколько строк вернуть (вызывающий берёт на одну больше).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Элементы страницы, не больше <paramref name="take"/>.</returns>
    Task<IReadOnlyList<Element>> ListBySetAsync(
        Guid setId,
        ElementCursor? after,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает все активные элементы набора по порядку.
    /// </summary>
    /// <remarks>
    /// <para>Объекты возвращаются без отслеживания изменений: чтобы изменить элемент,
    /// загрузите его отдельно через <see cref="FindAsync"/>.</para>
    /// <para>При перестановке из него берутся соседи перемещаемого элемента,
    /// при создании - последний элемент набора.</para>
    /// </remarks>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Активные элементы в порядке их рангов.</returns>
    Task<IReadOnlyList<Element>> ListForOrderingAsync(Guid setId, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает последний по порядку активный элемент набора.
    /// </summary>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Элемент или <see langword="null"/>, если набор пуст.</returns>
    Task<Element?> GetLastAsync(Guid setId, CancellationToken cancellationToken);

    /// <summary>
    /// Находит активный элемент набора и ставит его на отслеживание.
    /// </summary>
    /// <remarks>
    /// В отличие от <see cref="ListForOrderingAsync"/>, объект отслеживается:
    /// изменения, сделанные через его методы, попадут в базу при сохранении.
    /// </remarks>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="elementId">Идентификатор элемента.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Элемент или <see langword="null"/>, если элемента нет в наборе или он удалён.</returns>
    Task<Element?> FindAsync(Guid setId, Guid elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Считает число активных элементов набора.
    /// </summary>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество активных элементов.</returns>
    Task<int> CountActiveAsync(Guid setId, CancellationToken cancellationToken);

    /// <summary>
    /// Выполняет действие над составом набора в рамках транзакции с блокировкой строки набора.
    /// </summary>
    /// <remarks>
    /// <para>Реализация блокирует строку набора на время действия.
    /// За счёт этого параллельные изменения состава одного набора выполняются последовательно.</para>
    /// <para>Число активных элементов, переданное в действие, не изменится до конца транзакции.</para>
    /// <para>Все мутации состава идут через этот метод: проверка соседей и вычисление ранга вне
    /// лока оставляют окно, в котором состав успевает измениться.</para>
    /// <para>Если действие вернуло ошибку, транзакция откатывается.</para>
    /// </remarks>
    /// <typeparam name="TResult">Тип результата действия.</typeparam>
    /// <param name="setId">Набор, владение которым проверено.</param>
    /// <param name="action">Действие, получающее число активных элементов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат действия.</returns>
    Task<TResult> ExecuteLockedAsync<TResult>(
        Guid setId,
        Func<int, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken);

    /// <summary>
    /// Добавляет новый элемент в хранилище без сохранения.
    /// </summary>
    /// <param name="element">Новый элемент.</param>
    void Add(Element element);

    /// <summary>
    /// Мягко удаляет все элементы набора при удалении самого набора.
    /// </summary>
    /// <param name="setId">Удаляемый набор.</param>
    /// <param name="utcNow">Момент удаления в UTC.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество затронутых элементов.</returns>
    Task<int> MarkAllDeletedAsync(Guid setId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Сохраняет изменения, накопленные в хранилище.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
