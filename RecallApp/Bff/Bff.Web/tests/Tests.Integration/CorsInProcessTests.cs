using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// CORS для браузерных клиентов: разрешённый origin получает заголовки, чужой - нет,
/// не-браузерные запросы (без Origin) не затрагиваются.
/// Отдельно проверяется порядок middleware: preflight к /api/** не должен
/// упираться в SessionGuard (иначе браузер получил бы 401 на OPTIONS).
/// </summary>
[Trait("Category", "InProcess")]
public sealed class CorsInProcessTests : IAsyncLifetime
{
    private readonly BffApp _app = new();
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
    public async Task Preflight_AllowedOrigin_ReturnsNoContentWithCorsHeaders()
    {
        var response = await PreflightAsync("/session", "GET");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(BffApp.BrowserOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.True(Assert.Single(response.Headers.GetValues("Access-Control-Allow-Credentials")) == "true");
        Assert.True(response.Headers.Contains("Access-Control-Allow-Methods"));
    }

    [Fact]
    public async Task Preflight_AllowedOriginWithCsrfHeader_EchoesRequestedHeader()
    {
        var response = await PreflightAsync("/session", "POST", "x-csrf");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var allowed = string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers"));
        Assert.Contains("x-csrf", allowed, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Preflight_ApiPathAllowedOrigin_IsNotRejectedBySessionGuard()
    {
        // /api/** уходит в YARP и защищён SessionGuard: OPTIONS не должен дойти до гарда,
        // иначе анонимный preflight получил бы 401 и браузер не сделал бы сам запрос.
        var response = await PreflightAsync("/api/users/me", "GET");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(BffApp.BrowserOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task Preflight_UnknownOrigin_ReturnsWithoutCorsHeaders()
    {
        var response = await PreflightAsync("/session", "GET", origin: "https://evil.test");

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    [Fact]
    public async Task Session_RequestWithoutOrigin_ReturnsWithoutCorsHeaders()
    {
        // Не-браузерный клиент: заголовка Origin нет, CORS-middleware не вмешивается.
        var response = await _browser.GetAsync("/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private Task<HttpResponseMessage> PreflightAsync(
        string path,
        string method,
        string? requestHeader = null,
        string origin = BffApp.BrowserOrigin
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        if (requestHeader is not null)
            request.Headers.Add("Access-Control-Request-Headers", requestHeader);

        return _browser.SendAsync(request);
    }
}
