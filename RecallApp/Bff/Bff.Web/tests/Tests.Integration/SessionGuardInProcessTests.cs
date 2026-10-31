using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Граница сессии для API и поведение после отказа Keycloak на refresh:
/// /api/** без сессии должен возвращать 401 problem+json вместо 302, после Rejected - 401 session_expired,
/// не-API пути после Rejected продолжают запрос с анонимной личностью.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class SessionGuardInProcessTests : IAsyncLifetime
{
    private static readonly TimeSpan IntoRefreshZone = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(40);

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
    public async Task ApiEndpoint_WithoutSession_Returns401ProblemJsonInsteadOfRedirect()
    {
        var response = await _browser.GetAsync(TestSessionEndpoints.ApiEchoPath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("Location"));
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("not_authenticated", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_WithoutSession_RedirectsToKeycloak()
    {
        var response = await _browser.GetAsync("/login");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("auth.test", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ApiEndpoint_AfterKeycloakRejection_Returns401SessionExpiredAndDropsCookie()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));
        _app.Keycloak.Respond = _ =>
            Task.FromResult(FakeKeycloak.Json(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}"""));
        _app.Time.Advance(IntoRefreshZone);

        var response = await _browser.GetAsync(TestSessionEndpoints.ApiEchoPath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("session_expired", body.GetProperty("code").GetString());
        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(BffApp.SessionCookies(response)));

        // Cookie удалена: следующий запрос анонимен и получает not_authenticated.
        var nextResponse = await _browser.GetAsync(TestSessionEndpoints.ApiEchoPath);
        Assert.Equal(HttpStatusCode.Unauthorized, nextResponse.StatusCode);
        var nextBody = await nextResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("not_authenticated", nextBody.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ApiMutation_WithoutCsrfHeader_Returns403()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.PostAsync(TestSessionEndpoints.ApiEchoPath, content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("csrf_failed", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ApiMutation_WithCsrfHeader_Passes()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));
        _browser.DefaultRequestHeaders.Add("X-CSRF", "1");

        var response = await _browser.PostAsync(TestSessionEndpoints.ApiEchoPath, content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiRead_WithoutCsrfHeader_Passes()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.GetAsync(TestSessionEndpoints.ApiEchoPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Session_AfterKeycloakRejection_ReturnsAnonymousInSameResponse()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));
        _app.Keycloak.Respond = _ =>
            Task.FromResult(FakeKeycloak.Json(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}"""));
        _app.Time.Advance(IntoRefreshZone);

        var (response, body) = await BffApp.GetAppSessionAsync(_browser);

        Assert.False(body.GetProperty("isAuthenticated").GetBoolean());
        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(BffApp.SessionCookies(response)));
    }
}
