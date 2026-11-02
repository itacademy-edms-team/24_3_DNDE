using System.Xml.Linq;
using Accounts.Infrastructure;
using Accounts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tests.Architecture;

public sealed class ModuleBoundariesTests
{
    [Fact]
    public void AccountsProjectReferences_ForbiddenReferences_OnlyAllowedProjectsReferenced()
    {
        var allowed = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Accounts.Domain"] = [],
            ["Accounts.Contracts"] = ["Common.Application"],
            ["Accounts.Application"] = ["Accounts.Domain", "Common.Application"],
            ["Accounts.Infrastructure"] = ["Accounts.Application", "Accounts.Contracts", "Common.Application", "Common.Infrastructure"],
        };

        foreach (var (project, allowedRefs) in allowed)
        {
            var document = XDocument.Load(ProjectPath(project));
            var references = document.Descendants("ProjectReference")
                .Select(x => Path.GetFileNameWithoutExtension(x.Attribute("Include")!.Value))
                .ToArray();
            Assert.All(references, name => Assert.Contains(name, allowedRefs));
        }
    }

    [Fact]
    public void AccountsInfrastructure_ExportedTypes_OnlyModuleCompositionIsPublic()
    {
        var exported = typeof(AccountsModule).Assembly.GetExportedTypes()
            .Where(t => !typeof(Migration).IsAssignableFrom(t))
            .ToArray();
        Assert.Equal([typeof(AccountsModule)], exported);
    }

    [Fact]
    public void AccountsDbContext_Model_AllEntitiesAndForeignKeysInAccountsSchema()
    {
        using var context = new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);

        foreach (var entity in context.Model.GetEntityTypes())
        {
            Assert.Equal("accounts", entity.GetSchema());
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal("accounts", fk.PrincipalEntityType.GetSchema()));
        }
    }

    private static string ProjectPath(string projectName) =>
        Path.Combine(FindRoot(), "src", "Modules", "Accounts", projectName, $"{projectName}.csproj");

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "App.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException("Run architecture tests from the App checkout.");
    }
}
