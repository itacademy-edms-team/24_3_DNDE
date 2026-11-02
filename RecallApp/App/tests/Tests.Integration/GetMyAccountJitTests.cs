using System.Net.Http.Json;
using Accounts.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Tests.Integration;

/// <summary>Применяет миграции Accounts к app_db перед Database-тестами.</summary>
public sealed class DatabaseFixture
{
    public DatabaseFixture()
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var migrations = config.GetConnectionString("AccountsMigrations");
        if (!string.IsNullOrEmpty(migrations))
            AccountsModule.MigrateAccounts(migrations).GetAwaiter().GetResult();
    }
}

public sealed class GetMyAccountJitTests : IClassFixture<AppFactory>, IClassFixture<DatabaseFixture>
{
    private readonly AppFactory _factory;

    public GetMyAccountJitTests(AppFactory factory, DatabaseFixture database) => _factory = factory;

    [DatabaseFact]
    [Trait("Category", "Database")]
    public async Task GetMyAccount_Authenticated_JitProvisionsAndReturnsStableProfile()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");

        var first = await client.GetFromJsonAsync<AccountDto>("/api/users/me");
        var second = await client.GetFromJsonAsync<AccountDto>("/api/users/me");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.Id, second!.Id);
        Assert.Equal("Test User", first.DisplayName);
        Assert.Equal("testuser@example.com", first.Email);
        Assert.True(first.EmailVerified);
        Assert.True(first.Id != Guid.Empty);
    }

    private sealed record AccountDto(Guid Id, string DisplayName, string Email, bool EmailVerified);
}
