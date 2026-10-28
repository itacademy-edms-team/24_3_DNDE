namespace Tests.Integration.Helpers;

/// <summary>
/// Подключение к тестовому realm recallapp-test (Iam/README.md).
/// Значения приходят из переменных окружения KeycloakTests__*, которые задаёт compose-сервис bff-tests.
/// </summary>
public sealed record KeycloakTestSettings(
    string Authority,
    string ClientId,
    string ClientSecret,
    string Username,
    string Password
)
{
    private const string Prefix = "KeycloakTests__";

    /// <summary>Настройки из окружения или null, если хотя бы одной переменной нет.</summary>
    public static KeycloakTestSettings? FromEnvironment()
    {
        string? Read(string name) =>
            Environment.GetEnvironmentVariable(Prefix + name) is { Length: > 0 } value ? value : null;

        var authority = Read("Authority");
        var clientId = Read("ClientId");
        var clientSecret = Read("ClientSecret");
        var username = Read("Username");
        var password = Read("Password");

        return authority is null || clientId is null || clientSecret is null || username is null || password is null
            ? null
            : new KeycloakTestSettings(authority, clientId, clientSecret, username, password);
    }

    public static KeycloakTestSettings Required =>
        FromEnvironment()
        ?? throw new InvalidOperationException("Keycloak test settings are missing (KeycloakTests__*)");

    public string TokenEndpoint => $"{Authority}/protocol/openid-connect/token";
}

/// <summary>
/// Fact, который пропускается, если тестовый Keycloak не настроен (запуск из IDE без контейнера).
/// </summary>
public sealed class KeycloakFactAttribute : FactAttribute
{
    public KeycloakFactAttribute()
    {
        if (KeycloakTestSettings.FromEnvironment() is null)
            Skip = "Нужен тестовый Keycloak: запуск через docker compose --profile test run --rm bff-tests (tests/README.md)";
    }
}
