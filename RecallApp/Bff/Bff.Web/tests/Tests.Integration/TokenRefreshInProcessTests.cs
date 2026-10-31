using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Упреждающий refresh через весь конвейер BFF (cookie, стор, AuthenticationMiddleware, TokenRefreshMiddleware, контроллер).
/// Keycloak и часы через fake.
/// Сессия засевается с access на 5 минут: expires_at = вход + 4:30, refresh нужен после вход + 3:30.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class TokenRefreshInProcessTests : IAsyncLifetime
{
    private static readonly TimeSpan IntoRefreshZone = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(40);

    private readonly BffApp _app = new();
    private HttpClient _browser = null!;

    public async Task InitializeAsync()
    {
        _browser = _app.CreateBrowser();
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));
    }

    public async Task DisposeAsync()
    {
        _browser.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Session_AccessTokenFresh_DoesNotRefreshAndSetsNoCookie()
    {
        var (response, body) = await BffApp.GetAppSessionAsync(_browser);

        Assert.True(body.GetProperty("isAuthenticated").GetBoolean());
        Assert.Equal(0, _app.Keycloak.RequestCount);
        Assert.Empty(BffApp.SessionCookies(response));
    }

    [Fact]
    public async Task Session_AccessTokenNearExpiry_RefreshesOnceAndSetsOneCookie()
    {
        _app.Time.Advance(IntoRefreshZone);

        var (response, body) = await BffApp.GetAppSessionAsync(_browser);

        Assert.Equal(1, _app.Keycloak.RequestCount);
        Assert.Contains("refresh_token=initial-refresh", _app.Keycloak.RequestBodies.Single());
        Assert.Single(BffApp.SessionCookies(response));
        // Контроллер в том же запросе видит новый access (GetTokenAsync после refresh).
        Assert.Equal("refreshed-role", body.GetProperty("roles")[0].GetString());
    }

    [Fact]
    public async Task Session_AfterRefresh_StoresNewTokensAndRestartsWindow()
    {
        _app.Time.Advance(IntoRefreshZone);
        var refreshedAt = _app.Time.GetUtcNow();

        await BffApp.GetAppSessionAsync(_browser);
        var session = await BffApp.ReadSessionAsync(_browser);

        Assert.Equal("refreshed-refresh", session.Tokens["refresh_token"]);
        Assert.Equal("refreshed-id", session.Tokens["id_token"]);
        Assert.Equal("Bearer", session.Tokens["token_type"]);
        Assert.Equal(
            refreshedAt + TimeSpan.FromSeconds(300 - 30),
            DateTimeOffset.Parse(session.Tokens["expires_at"], System.Globalization.CultureInfo.InvariantCulture)
        );
        Assert.Equal(refreshedAt, session.IssuedUtc);
        Assert.Equal(refreshedAt + TimeSpan.FromMinutes(30), session.ExpiresUtc);
    }

    [Fact]
    public async Task Session_ResponseWithoutTokenType_KeepsTokenTypeFromLogin()
    {
        _app.Keycloak.Respond = _ => Task.FromResult(FakeKeycloak.Success(tokenType: null));
        _app.Time.Advance(IntoRefreshZone);

        await BffApp.GetAppSessionAsync(_browser);
        var session = await BffApp.ReadSessionAsync(_browser);

        Assert.Equal("Bearer", session.Tokens["token_type"]);
    }

    [Fact]
    public async Task Session_NextRefresh_UsesRotatedRefreshToken()
    {
        _app.Time.Advance(IntoRefreshZone);
        await BffApp.GetAppSessionAsync(_browser);

        _app.Keycloak.Respond = _ => Task.FromResult(FakeKeycloak.Success(refreshToken: "second-refresh"));
        _app.Time.Advance(IntoRefreshZone);
        await BffApp.GetAppSessionAsync(_browser);

        Assert.Equal(2, _app.Keycloak.RequestCount);
        Assert.Contains("refresh_token=refreshed-refresh", _app.Keycloak.RequestBodies.Last());
    }

    [Fact]
    public async Task Session_ParallelRequestsAfterIdleOverHalfWindow_RefreshOnceAndSetOneCookie()
    {
        // Сценарий 9: окно 6 минут, к моменту запросов прошло 3:40 из 6, то есть больше половины.
        // Встроенное продление cookie сработало бы на каждом запросе и записало бы в стор старые токены.
        await using var app = new BffApp { SessionIdle = TimeSpan.FromMinutes(6) };
        using var browser = app.CreateBrowser();
        await BffApp.SignInAsync(browser, new TestSession(TestJwt.Create("initial-role")));

        var release = new TaskCompletionSource();
        app.Keycloak.Respond = async _ =>
        {
            await release.Task;
            return FakeKeycloak.Success();
        };
        app.Time.Advance(IntoRefreshZone);

        var requests = Enumerable
            .Range(0, 5)
            .Select(i => BffApp.GetAppSessionAsync(browser, $"/session?i={i}"))
            .ToArray();
        // Даём всем запросам дойти до замка, пока первый держит его в ожидании Keycloak.
        await Task.Delay(200);
        release.SetResult();
        var responses = await Task.WhenAll(requests);

        Assert.Equal(1, app.Keycloak.RequestCount);
        Assert.All(responses, r => Assert.True(r.Body.GetProperty("isAuthenticated").GetBoolean()));
        // Остальные запросы ничего не пишут в cookie и стор: в сторе токены из refresh.
        Assert.Single(responses.SelectMany(r => BffApp.SessionCookies(r.Response)));
        Assert.Equal("refreshed-refresh", (await BffApp.ReadSessionAsync(browser)).Tokens["refresh_token"]);
    }

    [Fact]
    public async Task Session_KeycloakUnavailable_KeepsSessionAndBacksOff()
    {
        _app.Keycloak.Respond = _ => Task.FromResult(FakeKeycloak.Json(HttpStatusCode.BadGateway, "{}"));
        _app.Time.Advance(IntoRefreshZone);

        var (first, firstBody) = await BffApp.GetAppSessionAsync(_browser);
        var (_, secondBody) = await BffApp.GetAppSessionAsync(_browser);

        Assert.True(firstBody.GetProperty("isAuthenticated").GetBoolean());
        Assert.True(secondBody.GetProperty("isAuthenticated").GetBoolean());
        Assert.Empty(BffApp.SessionCookies(first));
        // Второй запрос в паузе 15 с: Keycloak не вызывается.
        Assert.Equal(1, _app.Keycloak.RequestCount);
        Assert.Equal("initial-refresh", (await BffApp.ReadSessionAsync(_browser)).Tokens["refresh_token"]);
    }

    [Fact]
    public async Task Session_BackoffElapsed_RetriesRefresh()
    {
        _app.Keycloak.Respond = _ => Task.FromException<HttpResponseMessage>(new TaskCanceledException("timeout"));
        _app.Time.Advance(IntoRefreshZone);
        await BffApp.GetAppSessionAsync(_browser);

        _app.Keycloak.Respond = _ => Task.FromResult(FakeKeycloak.Success());
        _app.Time.Advance(TimeSpan.FromSeconds(16));
        var (response, _) = await BffApp.GetAppSessionAsync(_browser);

        Assert.Equal(2, _app.Keycloak.RequestCount);
        Assert.Single(BffApp.SessionCookies(response));
    }

    [Fact]
    public async Task Session_KeycloakInvalidClient_KeepsSessionAndBacksOff()
    {
        // invalid_client -> сбой конфигурации, а не сессии: выходить не нужно.
        _app.Keycloak.Respond = _ =>
            Task.FromResult(FakeKeycloak.Json(HttpStatusCode.Unauthorized, """{"error":"invalid_client"}"""));
        _app.Time.Advance(IntoRefreshZone);

        var (first, firstBody) = await BffApp.GetAppSessionAsync(_browser);
        var (_, secondBody) = await BffApp.GetAppSessionAsync(_browser);

        Assert.True(firstBody.GetProperty("isAuthenticated").GetBoolean());
        Assert.True(secondBody.GetProperty("isAuthenticated").GetBoolean());
        Assert.Empty(BffApp.SessionCookies(first));
        // Второй запрос в паузе 15 с: Keycloak не вызывается повторно.
        Assert.Equal(1, _app.Keycloak.RequestCount);
        Assert.Equal("initial-refresh", (await BffApp.ReadSessionAsync(_browser)).Tokens["refresh_token"]);
    }

    [Fact]
    public async Task Session_ParallelRequestsAfterRejection_KeycloakCalledOnceAndAllSignOut()
    {
        // А2/Б18: ждущие замок запросы не должны повторять обращение к Keycloak со старым
        // refresh-токеном: один POST на пачку, остальные выходят по маркеру отказа.
        var release = new TaskCompletionSource();
        _app.Keycloak.Respond = async _ =>
        {
            await release.Task;
            return FakeKeycloak.Json(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");
        };
        _app.Time.Advance(IntoRefreshZone);

        var requests = Enumerable
            .Range(0, 5)
            .Select(i => BffApp.GetAppSessionAsync(_browser, $"/session?i={i}"))
            .ToArray();
        await Task.Delay(200);
        release.SetResult();
        var responses = await Task.WhenAll(requests);

        Assert.Equal(1, _app.Keycloak.RequestCount);
        Assert.All(
            responses,
            r => Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(BffApp.SessionCookies(r.Response)))
        );

        var (_, nextBody) = await BffApp.GetAppSessionAsync(_browser);
        Assert.False(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Session_KeycloakRejects_SignsOutSession()
    {
        _app.Keycloak.Respond = _ =>
            Task.FromResult(FakeKeycloak.Json(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}"""));
        _app.Time.Advance(IntoRefreshZone);

        var (response, body) = await BffApp.GetAppSessionAsync(_browser);
        var (_, nextBody) = await BffApp.GetAppSessionAsync(_browser);

        // Guard middleware сбрасывает личность ещё в запросе с отказом: /session сразу анонимен.
        Assert.False(body.GetProperty("isAuthenticated").GetBoolean());
        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(BffApp.SessionCookies(response)));
        Assert.False(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Refresh_ClientAbortsWhileWaitingForLock_WaitCancelsAndLockReleased()
    {
        var release = new TaskCompletionSource();
        _app.Keycloak.Respond = async _ =>
        {
            await release.Task;
            return FakeKeycloak.Success();
        };
        _app.Time.Advance(IntoRefreshZone);

        // Первый запрос держит замок, ожидая Keycloak; второй ждёт замок и обрывается.
        var holder = BffApp.GetAppSessionAsync(_browser, "/session?i=holder");
        await Task.Delay(200);
        using var cts = new CancellationTokenSource();
        var waiter = _browser.GetAsync("/session?i=waiter", cts.Token);
        await Task.Delay(200);
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter);

        release.SetResult();
        var (_, holderBody) = await holder;
        Assert.True(holderBody.GetProperty("isAuthenticated").GetBoolean());

        // Замок возвращён: следующий запрос проходит без зависания и видит живую сессию.
        var (_, nextBody) = await BffApp.GetAppSessionAsync(_browser);
        Assert.True(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Session_NoRefreshTokenInSession_SignsOutWithoutCallingKeycloak()
    {
        using var browser = _app.CreateBrowser();
        await BffApp.SignInAsync(browser, new TestSession(TestJwt.Create("initial-role"), RefreshToken: null));
        _app.Time.Advance(IntoRefreshZone);

        await BffApp.GetAppSessionAsync(browser);
        var (_, nextBody) = await BffApp.GetAppSessionAsync(browser);

        Assert.Equal(0, _app.Keycloak.RequestCount);
        Assert.False(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Session_WithoutBffSessionId_SignsOutWithoutCallingKeycloak()
    {
        using var browser = _app.CreateBrowser();
        await BffApp.SignInAsync(
            browser,
            new TestSession(TestJwt.Create("initial-role"), WithoutSessionId: true)
        );
        _app.Time.Advance(IntoRefreshZone);

        await BffApp.GetAppSessionAsync(browser);
        var (_, nextBody) = await BffApp.GetAppSessionAsync(browser);

        Assert.Equal(0, _app.Keycloak.RequestCount);
        Assert.False(nextBody.GetProperty("isAuthenticated").GetBoolean());
    }

    [Fact]
    public async Task Session_WindowExpired_IsAnonymousWithoutRefresh()
    {
        _app.Time.Advance(TimeSpan.FromMinutes(31));

        var (_, body) = await BffApp.GetAppSessionAsync(_browser);

        Assert.False(body.GetProperty("isAuthenticated").GetBoolean());
        Assert.Equal(0, _app.Keycloak.RequestCount);
    }

    [Fact]
    public async Task Session_RememberMeRefresh_KeepsPersistentCookieWithLongWindow()
    {
        using var browser = _app.CreateBrowser();
        await BffApp.SignInAsync(browser, new TestSession(TestJwt.Create("initial-role"), RememberMe: true));
        _app.Time.Advance(IntoRefreshZone);
        var refreshedAt = _app.Time.GetUtcNow();

        var (response, _) = await BffApp.GetAppSessionAsync(browser);
        var session = await BffApp.ReadSessionAsync(browser);

        Assert.Contains("expires=", Assert.Single(BffApp.SessionCookies(response)), StringComparison.OrdinalIgnoreCase);
        Assert.True(session.IsPersistent);
        Assert.Equal(refreshedAt + TimeSpan.FromDays(30), session.ExpiresUtc);
    }
}
