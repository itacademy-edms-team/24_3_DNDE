using Host.Configuration;
using Microsoft.Extensions.Options;

namespace Tests.Unit.Helpers;

/// <summary>Настройки OIDC для тестов: значения по умолчанию из <see cref="OidcOptions"/> плюс фиктивный клиент.</summary>
internal static class TestOidcOptions
{
    public const string Authority = "https://auth.test/realms/recallapp";
    public const string ClientId = "Bff.Web";
    public const string ClientSecret = "test-secret";

    public static OidcOptions Create() =>
        new()
        {
            Authority = Authority,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
        };

    public static IOptions<OidcOptions> Wrap(OidcOptions? options = null) =>
        Options.Create(options ?? Create());
}
