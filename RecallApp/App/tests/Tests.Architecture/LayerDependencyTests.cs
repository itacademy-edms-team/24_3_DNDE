using NetArchTest.Rules;

namespace Tests.Architecture;

/// <summary>
/// Границы слоёв модулей, проверенные по NetArchTest в IL:
/// фактические типовые зависимости из скомпилированных сборок.
/// </summary>
/// <remarks>
/// В случае добавления новых модулей тест упадёт и потребует ручного обновления для нового модуля.
/// Также он упадёт, если в существующих слоях появятся недопустимые связи.
/// </remarks>
public sealed class LayerDependencyTests
{
    // Common - общий kernel, а не модуль: входит в forbidden только там, где запрещается соответствующий слой.
    // Application/Infrastructure/Contracts всех модулей, кроме Common
    private static readonly string[] ModuleApplicationInfrastructureContracts =
    [
        "Accounts.Application", "Accounts.Infrastructure", "Accounts.Contracts",
        "Sets.Application", "Sets.Infrastructure", "Sets.Contracts",
        "Media.Application", "Media.Infrastructure", "Media.Contracts",
    ];

    // Infrastructure всех модулей + Common.Infrastructure.
    private static readonly string[] AllInfrastructure =
    [
        "Accounts.Infrastructure", "Sets.Infrastructure",
        "Media.Infrastructure", "Common.Infrastructure",
    ];

    // Domain и Infrastructure модулей + Common.Infrastructure (без Common.Domain - он разрешён).
    private static readonly string[] ModuleDomainAndInfrastructure =
    [
        "Accounts.Domain", "Accounts.Infrastructure",
        "Sets.Domain", "Sets.Infrastructure",
        "Media.Domain", "Media.Infrastructure",
        "Common.Infrastructure",
    ];

    // Domain, Application, Contracts всех модулей, кроме Common.
    private static readonly string[] ModuleDomainApplicationContracts =
    [
        "Accounts.Domain", "Accounts.Application", "Accounts.Contracts",
        "Sets.Domain", "Sets.Application", "Sets.Contracts",
        "Media.Domain", "Media.Application", "Media.Contracts",
    ];

    // Domain, Application, Infrastructure всех модулей, кроме Common.
    private static readonly string[] ModuleDomainApplicationInfrastructure =
    [
        "Accounts.Domain", "Accounts.Application", "Accounts.Infrastructure",
        "Sets.Domain", "Sets.Application", "Sets.Infrastructure",
        "Media.Domain", "Media.Application", "Media.Infrastructure",
    ];

    [Fact]
    public void Modules_List_IsExactlyKnownSet()
    {
        var modules = new DirectoryInfo(Path.Combine(FindRoot(), "src", "Modules"))
            .GetDirectories()
            .Select(module => module.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["Accounts", "Media", "Sets"], modules);
    }

    [Fact]
    public void Domain_DoesNotDependOnApplicationInfrastructureOrContracts()
    {
        var forbidden = ModuleApplicationInfrastructureContracts
            .Concat(["Common.Application", "Common.Infrastructure"])
            .ToArray();

        foreach (var project in new[] { "Accounts.Domain", "Sets.Domain", "Media.Domain" })
            AssertNoDependencies(project, forbidden);
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructure()
    {
        foreach (var project in new[] { "Accounts.Application", "Sets.Application", "Media.Application" })
            AssertNoDependencies(project, AllInfrastructure);
    }

    [Fact]
    public void Contracts_DoNotDependOnDomainOrInfrastructure()
    {
        AssertNoDependencies("Media.Contracts", ModuleDomainAndInfrastructure);
    }

    [Fact]
    public void Modules_DoNotDependOnOtherModulesDomainApplicationOrInfrastructure()
    {
        var modules = new (string Prefix, string[] Layers)[]
        {
            ("Accounts", ["Accounts.Domain", "Accounts.Application", "Accounts.Infrastructure"]),
            ("Sets", ["Sets.Domain", "Sets.Application", "Sets.Infrastructure"]),
            ("Media", ["Media.Domain", "Media.Application", "Media.Infrastructure"]),
        };

        foreach (var (prefix, layers) in modules)
        {
            var foreign = ModuleDomainApplicationInfrastructure
                .Where(mn => !mn.StartsWith(prefix + ".", StringComparison.Ordinal))
                .ToArray();

            foreach (var layer in layers)
                AssertNoDependencies(layer, foreign);
        }
    }

    [Fact]
    public void Api_DependsOnlyOnInfrastructureAndCommon()
    {
        AssertNoDependencies("Api", ModuleDomainApplicationContracts);
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "App.sln"))) return directory.FullName;

        throw new DirectoryNotFoundException("Run architecture tests from the App checkout.");
    }

    private static void AssertNoDependencies(string project, string[] forbidden)
    {
        var result = Types.FromFile($"{project}.dll")
            .Should()
            .NotHaveDependencyOnAny(forbidden)
            .GetResult();

        var failing = result.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(result.IsSuccessful, $"{project}: forbidden dependencies -> {string.Join(", ", failing)}");
    }
}
