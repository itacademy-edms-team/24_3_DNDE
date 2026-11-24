using Sets.Application.Sets;
using Sets.Domain.Sets;

namespace Sets.Application.Abstractions;

/// <summary>
/// Определяет хранилище наборов.
/// </summary>
/// <remarks>
/// Все выборки ограничены владельцем: чужие и удалённые наборы не возвращаются.
/// </remarks>
public interface ISetRepository
{
    /// <summary>
    /// Возвращает страницу наборов владельца в порядке <c>created_at DESC, id DESC</c>.
    /// </summary>
    /// <param name="ownerId">Владелец из контекста запроса.</param>
    /// <param name="after">Курсор: последний набор предыдущей страницы; если null - первая страница.</param>
    /// <param name="take">Сколько строк вернуть.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Наборы страницы в количестве не больше <paramref name="take"/> штук, в порядке сортировки.
    /// </returns>
    Task<IReadOnlyList<Set>> ListOwnedAsync(
        Guid ownerId,
        SetCursor? after,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Находит набор владельца по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор набора.</param>
    /// <param name="ownerId">Владелец набора.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Набор или <see langword="null"/>,
    /// если набор не существует, удалён или принадлежит другому пользователю.
    /// </returns>
    Task<Set?> FindOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>
    /// Добавляет новый набор в хранилище без сохранения.
    /// </summary>
    /// <param name="set">Новый набор.</param>
    void Add(Set set);

    /// <summary>
    /// Сохраняет изменения, накопленные в хранилище.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
