using Sets.Domain.Sets;

namespace Sets.Application.Abstractions;

/// <summary>
/// Доступ к наборам в схеме sets. Выборки ограничены владельцем: чужие строки не возвращаются.
/// </summary>
public interface ISetRepository
{
    Task<IReadOnlyList<Set>> ListOwnedAsync(Guid ownerId, CancellationToken cancellationToken);

    Task<Set?> FindOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);

    void Add(Set set);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
