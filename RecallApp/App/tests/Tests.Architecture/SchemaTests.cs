using Accounts.Infrastructure.Data;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Sets.Domain.Sets;
using Sets.Infrastructure.Data;

namespace Tests.Architecture;

/// <summary>
/// Схемы БД модулей: все сущности и FK лежат в своей схеме, межсхемных связей нет (ADR 0036).
/// </summary>
public sealed class SchemaTests
{
    [Fact]
    public void AccountsDbContext_Model_AllEntitiesAndForeignKeysInAccountsSchema() =>
        AssertAllEntitiesInSchema(
            () => new AccountsDbContext(
                new DbContextOptionsBuilder<AccountsDbContext>().UseNpgsql("Host=localhost;Database=unused").Options),
            "accounts");

    [Fact]
    public void SetsDbContext_Model_AllEntitiesAndForeignKeysInSetsSchema() =>
        AssertAllEntitiesInSchema(
            () => new SetsDbContext(
                new DbContextOptionsBuilder<SetsDbContext>().UseNpgsql("Host=localhost;Database=unused").Options),
            "sets");

    [Fact]
    public void MediaDbContext_Model_AllEntitiesAndForeignKeysInMediaSchema() =>
        AssertAllEntitiesInSchema(
            () => new MediaDbContext(
                new DbContextOptionsBuilder<MediaDbContext>().UseNpgsql("Host=localhost;Database=unused").Options),
            "media");

    [Fact]
    public void SetsDbContext_Model_OwnerHasNoForeignKey_ModulesDoNotShareSchemas()
    {
        using var context = new SetsDbContext(
            new DbContextOptionsBuilder<SetsDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);

        var set = context.Model.FindEntityType(typeof(Set))!;

        // Владелец — uuid без FK на accounts.users: межсхемных связей нет (ADR 0036).
        Assert.Empty(set.GetForeignKeys());
        Assert.Equal("owner_id", set.FindProperty(nameof(Set.OwnerId))!.GetColumnName());
    }

    private static void AssertAllEntitiesInSchema(Func<DbContext> createContext, string schema)
    {
        using var context = createContext();
        var entities = context.Model.GetEntityTypes().ToArray();

        Assert.NotEmpty(entities);
        foreach (var entity in entities)
        {
            Assert.Equal(schema, entity.GetSchema());
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(schema, fk.PrincipalEntityType.GetSchema()));
        }
    }
}
