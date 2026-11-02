using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Integration;

public sealed class GetMyAccountTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GetMyAccountTests(WebApplicationFactory<Program> factory) => _factory = factory
        .WithWebHostBuilder(builder => builder
            .UseSetting("ConnectionStrings:Database", "Host=localhost;Database=unused")
            .UseSetting("Oidc:Authority", "http://localhost/realms/recallapp")
            .UseSetting("Oidc:RequireHttpsMetadata", "false"));

    [Fact]
    public async Task GetMyAccount_Anonymous_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
