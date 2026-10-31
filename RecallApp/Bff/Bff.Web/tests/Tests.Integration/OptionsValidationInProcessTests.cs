using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Проверка связанных опций на старте хоста: таймаут Keycloak не ниже порога refresh -> приложение не стартует.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class OptionsValidationInProcessTests
{
    [Fact]
    public void Startup_KeycloakTimeoutNotBelowRefreshThreshold_AppDoesNotStart()
    {
        var factory = new BffApp().WithWebHostBuilder(builder =>
            builder.UseSetting(
                "Oidc:TokenRefresh:RefreshThreshold",
                TimeSpan.FromSeconds(2).ToString("c", CultureInfo.InvariantCulture)
            )
        );

        // Таймаут по умолчанию 5 с >= порог 2 с: валидация падает при старте хоста.
        var exception = Record.Exception(() =>
        {
            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }
            );
        });

        Assert.NotNull(exception);
        Assert.Contains("KeycloakTimeout", exception.ToString(), StringComparison.Ordinal);
    }
}
