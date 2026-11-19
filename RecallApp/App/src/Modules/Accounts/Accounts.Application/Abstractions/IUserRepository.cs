using Accounts.Domain;

namespace Accounts.Application.Abstractions;

/// <summary>
/// Определяет хранилище доменных пользователей.
/// </summary>
/// <remarks>
/// Репозиторий только для чтения: создание строки - задача JIT-провижининга,
/// который выполняется при первом доменном запросе.
/// </remarks>
public interface IUserRepository
{
    Task<User?> FindByKeycloakSubAsync(Guid keycloakSub, CancellationToken cancellationToken);
}
