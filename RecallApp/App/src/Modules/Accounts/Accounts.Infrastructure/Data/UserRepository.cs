using Accounts.Application.Abstractions;
using Accounts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Infrastructure.Data;

/// <summary>
/// Реализация хранилища доменных пользователей поверх EF Core.
/// </summary>
/// <remarks>
/// Чтение идёт без отслеживания изменений:
/// репозиторий только возвращает пользователя, запись выполняет провижининг.
/// </remarks>
internal sealed class UserRepository(AccountsDbContext db) : IUserRepository
{
    public Task<User?> FindByKeycloakSubAsync(Guid keycloakSub, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.KeycloakSub == keycloakSub, cancellationToken);
}
