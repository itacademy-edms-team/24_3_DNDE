using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Refresh против настоящего Keycloak (тестовый realm recallapp-test).
/// Сессия засевается токенами Direct Access Grants, часы BFF поддельные: refresh запускается сдвигом часов,
/// а Keycloak проверяет протокол, ротацию refresh-токенов и учётные данные клиента.
/// </summary>
[Trait("Category", "Keycloak")]
public sealed class TokenRefreshKeycloakTests
{
    private static readonly TimeSpan IntoRefreshZone = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(40);

    private static async Task<HttpClient> SignInWithKeycloakTokensAsync(BffApp app, KeycloakTokens tokens)
    {
        var browser = app.CreateBrowser();
        await BffApp
            .SignInAsync(
                browser,
                new TestSession(
                    tokens.AccessToken,
                    tokens.RefreshToken,
                    TimeSpan.FromSeconds(tokens.ExpiresIn)
                )
            )
            .ConfigureAwait(false);
        return browser;
    }

    [KeycloakFact]
    public async Task Session_RealKeycloakRefresh_StoresRotatedTokensAndKeepsRoles()
    {
        var settings = KeycloakTestSettings.Required;
        var login = await KeycloakDirectGrant.LoginAsync(settings);
        await using var app = new BffApp { RealKeycloak = settings };
        using var browser = await SignInWithKeycloakTokensAsync(app, login);

        app.Time.Advance(IntoRefreshZone);
        var (response, body) = await BffApp.GetAppSessionAsync(browser);
        var session = await BffApp.ReadSessionAsync(browser);

        Assert.True(body.GetProperty("isAuthenticated").GetBoolean());
        Assert.Contains("app-user", body.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));
        Assert.Single(BffApp.SessionCookies(response));
        Assert.NotEqual(login.RefreshToken, session.Tokens["refresh_token"]);
        Assert.NotEqual(login.AccessToken, session.Tokens["access_token"]);
        Assert.Equal("Bearer", session.Tokens["token_type"]);
    }

    [KeycloakFact]
    public async Task Session_RefreshTokenAlreadyUsed_SignsOut()
    {
        // Revoke Refresh Token ON, Max Reuse 0: после использования refresh-токен отозван.
        var settings = KeycloakTestSettings.Required;
        var login = await KeycloakDirectGrant.LoginAsync(settings);
        await KeycloakDirectGrant.RefreshAsync(settings, login.RefreshToken);
        await using var app = new BffApp { RealKeycloak = settings };
        using var browser = await SignInWithKeycloakTokensAsync(app, login);

        app.Time.Advance(IntoRefreshZone);
        var (response, _) = await BffApp.GetAppSessionAsync(browser);
        var (_, nextBody) = await BffApp.GetAppSessionAsync(browser);

        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(BffApp.SessionCookies(response)));
        Assert.False(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }

    [KeycloakFact]
    public async Task Session_WrongClientSecret_SignsOut()
    {
        var settings = KeycloakTestSettings.Required;
        var login = await KeycloakDirectGrant.LoginAsync(settings);
        await using var app = new BffApp { RealKeycloak = settings with { ClientSecret = "wrong-secret" } };
        using var browser = await SignInWithKeycloakTokensAsync(app, login);

        app.Time.Advance(IntoRefreshZone);
        await BffApp.GetAppSessionAsync(browser);
        var (_, nextBody) = await BffApp.GetAppSessionAsync(browser);

        Assert.False(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }
}
