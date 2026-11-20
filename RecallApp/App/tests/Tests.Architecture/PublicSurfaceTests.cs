using System.Reflection;
using Accounts.Infrastructure;
using Media.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sets.Infrastructure;

namespace Tests.Architecture;

/// <summary>
/// Публичная поверхность Infrastructure-сборок: публичным делаем только класс композиции модуля.
/// </summary>
/// <remarks>
/// Тесты учитывают code-first миграции, у которых тип тоже публичный,
/// и исключают их из конечного списка типов на проверку.
/// </remarks>
public sealed class PublicSurfaceTests
{
    [Fact]
    public void AccountsInfrastructure_OnlyModuleCompositionIsPublic() =>
        AssertOnlyModulePublic(typeof(AccountsModule).Assembly, typeof(AccountsModule));

    [Fact]
    public void SetsInfrastructure_OnlyModuleCompositionIsPublic() =>
        AssertOnlyModulePublic(typeof(SetsModule).Assembly, typeof(SetsModule));

    [Fact]
    public void MediaInfrastructure_OnlyModuleCompositionIsPublic() =>
        AssertOnlyModulePublic(typeof(MediaModule).Assembly, typeof(MediaModule));

    private static void AssertOnlyModulePublic(Assembly assembly, Type module) =>
        Assert.Equal([module], assembly.GetExportedTypes()
            // Исключаем производные публичные типы от Migration.
            // Миграции в Infrastructure-проектах публичные: исключаем их из конечного массива публичных типов.
            .Where(type => !typeof(Migration).IsAssignableFrom(type))
            .ToArray());
}
