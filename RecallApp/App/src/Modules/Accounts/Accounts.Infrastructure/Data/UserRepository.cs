using Accounts.Application.Abstractions;
using Accounts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Infrastructure.Data;

internal sealed class UserRepository(AccountsDbContext db) : IUserRepository
{
    public Task<User?> FindByKeycloakSubAsync(Guid keycloakSub, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.KeycloakSub == keycloakSub, cancellationToken);
}
