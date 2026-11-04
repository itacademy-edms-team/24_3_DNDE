using Common.Application.Provisioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Security.Claims;

namespace Accounts.Infrastructure.Data;

/// <summary>
/// Идемпотентный upsert доменной строки по UNIQUE(keycloak_sub) с перечитыванием (ADR 0020).
/// Гонка первого запроса разрешается constraint'ом: опоздавший обновит только кэш email.
/// </summary>
internal sealed class UserProvisioner(AccountsDbContext db, TimeProvider clock, ILogger<UserProvisioner> logger)
    : IDomainProvisioner
{
    public async Task EnsureProvisionedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var subClaim = user.FindFirst("sub")?.Value;
        if (!Guid.TryParse(subClaim, out var keycloakSub))
        {
            if (subClaim is null)
                logger.LogError("Claim 'sub' not found in JWT, check MapInboundClaims and token content");
            else
                logger.LogWarning("Claim 'sub' is not UUID: JIT-provisioning skipped");

            return;
        }
        var email = user.FindFirst("email")?.Value ?? string.Empty;
        var displayName = user.FindFirst("name")?.Value ?? string.Empty;

        var now = clock.GetUtcNow().UtcDateTime;
        const string sql = """
            INSERT INTO accounts.users (id, keycloak_sub, email, display_name, created_at, updated_at)
            VALUES (gen_random_uuid(), @keycloak_sub, @email, @display_name, @created_at, @updated_at)
            ON CONFLICT (keycloak_sub) DO UPDATE SET email = EXCLUDED.email
            """;

        // ON CONFLICT: существующая строка не перезаписывается, кроме кэша email (User.RefreshEmailContact).
        await db.Database.ExecuteSqlRawAsync(sql,
            new object[]
            {
                new NpgsqlParameter<Guid>("keycloak_sub", keycloakSub),
                new NpgsqlParameter<string>("email", email.Trim().ToLowerInvariant()),
                new NpgsqlParameter<string>("display_name", displayName.Trim()),
                new NpgsqlParameter<DateTime>("created_at", now),
                new NpgsqlParameter<DateTime>("updated_at", now),
            },
            cancellationToken);

        // Перечитывание: DbContext не отслеживает результат ExecuteSql, строка нужна ниже по конвейеру.
        var tracked = await db.Users
            .SingleOrDefaultAsync(u => u.KeycloakSub == keycloakSub, cancellationToken);
        if (tracked is not null)
            db.Entry(tracked).State = EntityState.Detached;
    }
}
