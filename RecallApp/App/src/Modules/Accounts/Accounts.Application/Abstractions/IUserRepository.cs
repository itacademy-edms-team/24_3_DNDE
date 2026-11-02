using Accounts.Domain;

namespace Accounts.Application.Abstractions;

/// <summary>Чтение доменных пользователей.</summary>
public interface IUserRepository
{
    Task<User?> FindByKeycloakSubAsync(Guid keycloakSub, CancellationToken cancellationToken);
}
