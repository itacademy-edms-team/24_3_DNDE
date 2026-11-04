using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration;

public sealed class GetMyAccountTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public GetMyAccountTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task GetMyAccount_Anonymous_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyAccount_MalformedSubClaim_Returns400ProblemJson()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, "not-a-uuid");

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        // Код домена в title, происхождение - extension source (ADR 0031).
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("InvalidKeycloakSub", problem.GetProperty("title").GetString());
        Assert.Equal("app", problem.GetProperty("source").GetString());
        Assert.Equal("user.keycloakSub", problem.GetProperty("target").GetString());
    }
}
