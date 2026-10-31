using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Контракт endpoint-ов входа и выхода:
/// /login при открытой сессии сразу возвращает на returnUrl без новой сессии,
/// /logout работает только по POST и без живой сессии делает только локальный redirect.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class LoginLogoutInProcessTests : IAsyncLifetime
{
    private readonly BffApp _app = new();
    private HttpClient _browser = null!;

    public async Task InitializeAsync()
    {
        _browser = _app.CreateBrowser();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _browser.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithOpenSession_RedirectsToReturnUrlWithoutNewSession()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.GetAsync("/login?returnUrl=/dashboard");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/dashboard", response.Headers.Location?.ToString());
        // Повторный Challenge не вызывался: новой cookie (и новой записи в сторе за ней) нет.
        Assert.Empty(BffApp.SessionCookies(response));
    }

    [Fact]
    public async Task Logout_Get_DoesNotSignOut()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.GetAsync("/logout");

        // Маршрут есть только под POST: endpoint routing отвечает 405.
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        var (_, body) = await BffApp.GetAppSessionAsync(_browser);
        Assert.True(body.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Logout_Post_SignsOut()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.PostAsync("/logout", content: null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(BffApp.SessionCookies(response)));
        var (_, body) = await BffApp.GetAppSessionAsync(_browser);
        Assert.False(body.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Logout_PostWithoutSession_RedirectsLocallyWithoutKeycloak()
    {
        var response = await _browser.PostAsync("/logout", content: null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
    }
}
