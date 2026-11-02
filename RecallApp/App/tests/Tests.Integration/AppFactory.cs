using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Integration;

/// <summary>
/// Фабрика тестового хоста приложения.
/// </summary>
public sealed class AppFactory : WebApplicationFactory<Program>
{
	// Пока без реальной бд в контейнере, но конфигурацию всё равно нужно задать
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder
        .UseSetting("ConnectionStrings:Database", "Host=localhost;Database=unused")
        .UseSetting("Oidc:Authority", "http://localhost/realms/recallapp")
        .UseSetting("Oidc:RequireHttpsMetadata", "false")
        .UseEnvironment("Development");
}
