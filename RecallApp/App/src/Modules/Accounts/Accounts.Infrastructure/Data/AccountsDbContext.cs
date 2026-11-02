using Accounts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Infrastructure.Data;

internal sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("accounts");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly);
    }
}
