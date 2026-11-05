using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Редиректы «наружу» из BFF идут только в SPA (ADR 0030): BFF живёт на своём origin'е
/// и своей страницы не имеет, поэтому returnUrl склеивается с Spa:BaseUrl, а внешние адреса отбрасываются.
/// </summary>
[Trait("Category", "InProcess")]
public sealed class SpaRedirectInProcessTests : IAsyncLifetime
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
    public async Task Root_WithoutSession_RedirectsToSpa()
    {
        var response = await _browser.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"{BffApp.SpaBaseUrl}/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Login_WithOpenSession_KeepsSearchAndPathInSpaUrl()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.GetAsync("/login?returnUrl=%2Faccount%3Ftab%3Dsecurity");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(
            $"{BffApp.SpaBaseUrl}/account?tab=security",
            response.Headers.Location?.ToString()
        );
    }

    [Fact]
    public async Task Login_WithExternalReturnUrl_RedirectsToSpaRoot()
    {
        await BffApp.SignInAsync(_browser, new TestSession(TestJwt.Create("initial-role")));

        var response = await _browser.GetAsync("/login?returnUrl=https%3A%2F%2Fevil.test%2Fsteal");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"{BffApp.SpaBaseUrl}/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Logout_PostWithoutSession_RedirectsToSpa()
    {
        var response = await _browser.PostAsync("/logout", content: null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"{BffApp.SpaBaseUrl}/", response.Headers.Location?.ToString());
    }
}
