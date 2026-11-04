using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// HTTP-контракт ответов вне Development: необработанные исключения возвращаются в виде problem+json без stack trace,
/// состояние сессии не кэшируем.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class ResponseContractInProcessTests : IAsyncLifetime
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
    public async Task ApiEndpoint_UnhandledException_ReturnsProblemJsonWithoutStackTrace()
    {
        // /api/** за SessionGuardMiddleware: чтобы дойти до исключения, нужна сессия.
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.GetAsync(TestSessionEndpoints.ApiThrowPath);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("test exception marker", body);
        Assert.DoesNotContain(nameof(InvalidOperationException), body);
    }

    [Fact]
    public async Task Session_AnyState_RespondsWithNoStore()
    {
        var response = await _browser.GetAsync("/session");

        response.EnsureSuccessStatusCode();
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Session_Authenticated_ReturnsOnlyWhitelistedFields()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var (_, body) = await BffApp.GetAppSessionAsync(_browser);

        // sub и роли клиенту не отдаются: авторизация - по ответам монолита, диагностика - /debug/session.
        var fields = body.EnumerateObject().Select(p => p.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "email", "isAuthenticated", "name" }, fields);
    }
}
