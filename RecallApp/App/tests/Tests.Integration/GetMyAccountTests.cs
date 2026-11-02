using System.Net;

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
}
