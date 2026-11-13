using System.Reflection;
using System.Xml.Linq;
using Accounts.Infrastructure;
using Accounts.Infrastructure.Data;
using Media.Domain.Objects;
using Media.Infrastructure;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Sets.Domain.Sets;
using Sets.Infrastructure;
using Sets.Infrastructure.Data;

namespace Tests.Architecture;

/// <summary>
/// Тесты проверки границ модулей.
/// </summary>
public sealed class ModuleBoundariesTests
{
    private const string AccountsModule = "Accounts";
    private const string SetsModuleName = "Sets";
    private const string MediaModuleName = "Media";

    private static readonly Dictionary<string, ProjectRule> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Accounts.Domain"] = new(AccountsModule, []),
        ["Accounts.Contracts"] = new(AccountsModule, ["Common.Application"]),
        ["Accounts.Application"] = new(AccountsModule, ["Accounts.Domain", "Common.Application"]),
        ["Accounts.Infrastructure"] = new(AccountsModule,
            ["Accounts.Application", "Accounts.Contracts", "Common.Application", "Common.Infrastructure"]),
        ["Sets.Domain"] = new(SetsModuleName, []),
        ["Sets.Contracts"] = new(SetsModuleName, ["Common.Application"]),
        ["Sets.Application"] = new(SetsModuleName, ["Sets.Domain", "Common.Application", "Media.Contracts"]),
        ["Sets.Infrastructure"] = new(SetsModuleName,
            ["Sets.Application", "Sets.Contracts", "Common.Application", "Common.Infrastructure", "Media.Contracts"]),
        ["Media.Domain"] = new(MediaModuleName, []),
        ["Media.Contracts"] = new(MediaModuleName, ["Common.Application", "Common.Domain"]),
        ["Media.Application"] = new(MediaModuleName, ["Media.Domain", "Media.Contracts", "Common.Application"]),
        ["Media.Infrastructure"] = new(MediaModuleName,
            ["Media.Application", "Media.Contracts", "Common.Application", "Common.Infrastructure"]),
        ["Api"] = new(null, ["Accounts.Infrastructure", "Sets.Infrastructure", "Media.Infrastructure", "Common.Infrastructure"]),
    };

    [Fact]
    public void ModuleProjects_ForbiddenReferences_OnlyAllowedProjectsReferenced()
    {
        foreach (var (project, rule) in Rules)
        {
            var document = XDocument.Load(ProjectPath(project, rule.Module));
            var references = document.Descendants("ProjectReference")
                .Select(x => Path.GetFileNameWithoutExtension(x.Attribute("Include")!.Value.Replace('\\', '/')))
                .ToArray();

            Assert.All(references, name => Assert.Contains(name, rule.AllowedReferences));
        }
    }

    [Fact]
    public void ModuleAssemblies_ExportedTypes_OnlyModuleCompositionIsPublic()
    {
        Assert.Equal([typeof(AccountsModule)], ExportedTypes(typeof(AccountsModule).Assembly));
        Assert.Equal([typeof(SetsModule)], ExportedTypes(typeof(SetsModule).Assembly));
        Assert.Equal([typeof(MediaModule)], ExportedTypes(typeof(MediaModule).Assembly));
    }

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
    public void SetsDbContext_Model_OwnerHasNoForeignKey_ModulesDoNotShareSchemas()
    {
        using var context = new SetsDbContext(
            new DbContextOptionsBuilder<SetsDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);

        var set = context.Model.FindEntityType(typeof(Set))!;

        // Владелец — uuid без FK на accounts.users: межсхемных связей нет (ADR 0036).
        Assert.Empty(set.GetForeignKeys());
        Assert.Equal("owner_id", set.FindProperty(nameof(Set.OwnerId))!.GetColumnName());
    }

    [Fact]
    public void MediaDbContext_Model_AllEntitiesAndForeignKeysInMediaSchema() =>
        AssertAllEntitiesInSchema(
            () => new MediaDbContext(
                new DbContextOptionsBuilder<MediaDbContext>().UseNpgsql("Host=localhost;Database=unused").Options),
            "media");

    /// <summary>Якорь значений из ontology/media.md: политика загрузки и ссылки на чтение.</summary>
    [Fact]
    public void MediaConstraints_Values_MatchOntology()
    {
        Assert.Equal(10L * 1024 * 1024, MediaConstraints.MaxUploadBytes);
        Assert.Equal(25_000_000, MediaConstraints.MaxPixels);
        Assert.Equal(8000, MediaConstraints.MaxSide);
        Assert.Equal(400, MediaConstraints.ThumbMaxSide);
        Assert.Equal(1600, MediaConstraints.DisplayMaxSide);
        Assert.Equal(TimeSpan.FromMinutes(10), MediaConstraints.UploadWindow);
        Assert.Equal(TimeSpan.FromMinutes(15), MediaConstraints.ReadLinkLifetime);
    }

    /// <summary>
    /// Якорь значений из ontology/sets.md: изменение лимита — осознанное решение,
    /// а не случайная правка константы (ADR 0038).
    /// </summary>
    [Fact]
    public void SetConstraints_Values_MatchOntology()
    {
        Assert.Equal(120, SetConstraints.TitleMaxLength);
        Assert.Equal(2000, SetConstraints.DescriptionMaxLength);
    }

    private static Type[] ExportedTypes(Assembly assembly) =>
        assembly.GetExportedTypes()
            .Where(type => !typeof(Migration).IsAssignableFrom(type))
            .ToArray();

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

    private static string ProjectPath(string projectName, string? moduleName)
    {
        var relative = moduleName is null
            ? Path.Combine("src", projectName)
            : Path.Combine("src", "Modules", moduleName, projectName);

        return Path.Combine(FindRoot(), relative, $"{projectName}.csproj");
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "App.sln"))) return directory.FullName;

        throw new DirectoryNotFoundException("Run architecture tests from the App checkout.");
    }

    private sealed record ProjectRule(string? Module, string[] AllowedReferences);
}
