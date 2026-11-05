using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Диагностический маршрут сессии: отдаёт то, что SPA не видит (sub, роли из access-токена),
/// и существует только при включённом флаге Diagnostics:Session.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class DiagnosticsInProcessTests : IAsyncLifetime
{
    private readonly BffApp _app = new() { DiagnosticsSession = true };
    private HttpClient _browser = null!;

    public Task InitializeAsync()
    {
        _browser = _app.CreateBrowser();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _browser.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task DiagnosticsSession_Authenticated_ReturnsRolesFromAccessToken()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("app-user")));

        var (response, body) = await BffApp.GetAppSessionAsync(_browser, "/debug/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.True(body.GetProperty("isAuthenticated").GetBoolean());
        Assert.Equal("user-1", body.GetProperty("sub").GetString());
        Assert.Equal(["app-user"], body.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));
        Assert.Equal(
            DateTimeOffset.FromUnixTimeSeconds(1_767_225_600),
            body.GetProperty("accessTokenExpiresAt").GetDateTimeOffset()
        );
    }

    [Fact]
    public async Task DiagnosticsSession_Anonymous_ReturnsOnlyAuthenticationFlag()
    {
        var (_, body) = await BffApp.GetAppSessionAsync(_browser, "/debug/session");

        var fields = body.EnumerateObject().Select(p => p.Name).ToArray();
        Assert.Equal(["isAuthenticated"], fields);
        Assert.False(body.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task DiagnosticsSession_FlagDisabled_RespondsWithNotFound()
    {
        await using var app = new BffApp();
        using var browser = app.CreateBrowser();
        await BffApp.SignInAsync(browser, new TestSession(TestJwt.Create("app-user")));

        var response = await browser.GetAsync("/debug/session");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
