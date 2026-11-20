using Host.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Time.Testing;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary>
/// Типы claim'ов, по которым ASP.NET ищет имя и роли.
/// Защита от регрессии: при неверном типе claim'а проверки молча отказывают - User.Identity.Name вернёт null, IsInRole -
/// false, [Authorize(Roles = "...")] - 403 залогиненному пользователю, без исключений и записей в лог.
/// </summary>
public class ConfigureOidcOptionsClaimTypesTests
{
    private readonly OpenIdConnectOptions _options = new();

    public ConfigureOidcOptionsClaimTypesTests()
    {
        new ConfigureOidcOptions(
            TestOidcOptions.Wrap(TestOidcOptions.Create()),
            TestSpaOptions.Wrap(),
            new FakeTimeProvider()
        ).Configure(OpenIdConnectDefaults.AuthenticationScheme, _options);
    }

    [Fact]
    public void Configure_NameClaimType_UsesFullNameClaim()
    {
        Assert.Equal("name", _options.TokenValidationParameters.NameClaimType);
    }

    [Fact]
    public void Configure_RoleClaimType_UsesFlatRolesClaim()
    {
        Assert.Equal("roles", _options.TokenValidationParameters.RoleClaimType);
    }
}
