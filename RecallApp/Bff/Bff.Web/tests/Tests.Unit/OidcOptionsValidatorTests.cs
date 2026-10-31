using Host.Configuration;
using Microsoft.Extensions.Options;

namespace Tests.Unit;

/// <summary>
/// Стартовая проверка опций refresh: таймаут Keycloak обязан быть меньше порога refresh,
/// иначе ожидание под замком переживёт окно (регрессия сценария 9).
/// </summary>
public class OidcOptionsValidatorTests
{
    private readonly OidcOptionsValidator _validator = new();

    private static OidcOptions CreateOptions(TimeSpan keycloakTimeout, TimeSpan refreshThreshold) =>
        new()
        {
            Authority = "https://auth.test/realms/recallapp",
            ClientId = "Bff.Web",
            ClientSecret = "test-secret",
            TokenRefresh = { KeycloakTimeout = keycloakTimeout, RefreshThreshold = refreshThreshold },
        };

    [Fact]
    public void Validate_TimeoutBelowThreshold_Succeeds()
    {
        var result = _validator.Validate(Options.DefaultName, CreateOptions(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1)));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_TimeoutEqualToThreshold_Fails()
    {
        var result = _validator.Validate(
            Options.DefaultName,
            CreateOptions(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        );

        Assert.False(result.Succeeded);
        Assert.True(result.Failures!.Any(f => f.Contains("KeycloakTimeout", StringComparison.Ordinal)));
    }

    [Fact]
    public void Validate_TimeoutAboveThreshold_Fails()
    {
        var result = _validator.Validate(
            Options.DefaultName,
            CreateOptions(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(1))
        );

        Assert.False(result.Succeeded);
        Assert.True(result.Failures!.Any(f => f.Contains("KeycloakTimeout", StringComparison.Ordinal)));
    }
}
