using Sets.Application.Sets;
using Sets.Domain.Sets;

namespace Sets.Application.Abstractions;

/// <summary>
/// Доступ к наборам в схеме sets. Выборки ограничены владельцем: чужие строки не возвращаются.
/// </summary>
public interface ISetRepository
{
    /// <summary>
    /// Страница наборов владельца в порядке <c>created_at DESC, id DESC</c>.
    /// </summary>
    /// <param name="ownerId">Владелец из контекста запроса.</param>
    /// <param name="after">Курсор: последний набор предыдущей страницы; если null - первая страница.</param>
    /// <param name="take">Сколько строк вернуть.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyList<Set>> ListOwnedAsync(
        Guid ownerId,
        SetCursor? after,
        int take,
        CancellationToken cancellationToken);

    Task<Set?> FindOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);

    void Add(Set set);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
