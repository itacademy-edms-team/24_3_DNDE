using System.Net;
using Host.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary>
/// Классификация ответов token-endpoint: успех, окончательный отказ (400/401),
/// временный сбой (остальные ошибки, сеть, таймаут, битый JSON) и состав <see cref="RefreshedTokens"/> из ответа.
/// </summary>
public class KeycloakTokenClientTests
{
    private static KeycloakTokenClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler), TestOidcOptions.Wrap(), NullLogger<KeycloakTokenClient>.Instance);

    [Fact]
    public async Task RefreshAsync_AnyRefreshToken_PostsRefreshGrantWithClientCredentials()
    {
        var handler = StubHttpMessageHandler.Json(
            HttpStatusCode.OK,
            """{"access_token":"a","expires_in":300}"""
        );

        await CreateClient(handler).RefreshAsync("old-refresh");

        var (request, body) = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            $"{TestOidcOptions.Authority}/protocol/openid-connect/token",
            request.RequestUri!.ToString()
        );
        Assert.Contains("grant_type=refresh_token", body);
        Assert.Contains($"client_id={Uri.EscapeDataString(TestOidcOptions.ClientId)}", body);
        Assert.Contains($"client_secret={TestOidcOptions.ClientSecret}", body);
        Assert.Contains("refresh_token=old-refresh", body);
    }

    [Fact]
    public async Task RefreshAsync_FullSuccessResponse_ReturnsSuccessWithAllTokens()
    {
        var handler = StubHttpMessageHandler.Json(
            HttpStatusCode.OK,
            """
            {"access_token":"new-access","refresh_token":"new-refresh","id_token":"new-id",
             "token_type":"Bearer","expires_in":300,"refresh_expires_in":1800}
            """
        );

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Success, result.Outcome);
        Assert.Equal(
            new RefreshedTokens("new-access", "new-refresh", "new-id", "Bearer", TimeSpan.FromSeconds(300)),
            result.Tokens
        );
    }

    [Fact]
    public async Task RefreshAsync_ResponseWithoutOptionalTokens_ReturnsSuccessWithNullOptionalTokens()
    {
        var handler = StubHttpMessageHandler.Json(
            HttpStatusCode.OK,
            """{"access_token":"new-access","expires_in":300}"""
        );

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Success, result.Outcome);
        Assert.Null(result.Tokens!.RefreshToken);
        Assert.Null(result.Tokens.IdToken);
        Assert.Null(result.Tokens.TokenType);
    }

    // Нестроковое значение обязательного типа JsonElement.GetString() бросило бы исключение
    // мимо классификации исходов, поэтому такие поля считаем отсутствующими.
    [Theory]
    [InlineData("""{"access_token":"new-access","expires_in":300,"refresh_token":42,"id_token":true,"token_type":{}}""")]
    [InlineData("""{"access_token":"new-access","expires_in":300,"refresh_token":null,"id_token":null,"token_type":null}""")]
    public async Task RefreshAsync_OptionalTokensOfNonStringType_ReturnsSuccessWithNullOptionalTokens(string body)
    {
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Success, result.Outcome);
        Assert.Equal("new-access", result.Tokens!.AccessToken);
        Assert.Null(result.Tokens.RefreshToken);
        Assert.Null(result.Tokens.IdToken);
        Assert.Null(result.Tokens.TokenType);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task RefreshAsync_Status400Or401_ReturnsRejected(HttpStatusCode status)
    {
        var handler = StubHttpMessageHandler.Json(status, """{"error":"invalid_grant"}""");

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Rejected, result.Outcome);
        Assert.Null(result.Tokens);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task RefreshAsync_OtherErrorStatus_ReturnsUnavailable(HttpStatusCode status)
    {
        var handler = StubHttpMessageHandler.Json(status, "{}");

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Unavailable, result.Outcome);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("""{"expires_in":300}""")]
    [InlineData("""{"access_token":"a"}""")]
    [InlineData("""{"access_token":"a","expires_in":"300"}""")]
    [InlineData("""{"access_token":42,"expires_in":300}""")]
    public async Task RefreshAsync_MalformedSuccessResponse_ReturnsUnavailable(string body)
    {
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task RefreshAsync_NetworkError_ReturnsUnavailable()
    {
        var handler = StubHttpMessageHandler.Throws(new HttpRequestException("connection refused"));

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task RefreshAsync_Timeout_ReturnsUnavailable()
    {
        // Таймаут HttpClient приходит как TaskCanceledException.
        var handler = StubHttpMessageHandler.Throws(new TaskCanceledException("timeout"));

        var result = await CreateClient(handler).RefreshAsync("old-refresh");

        Assert.Equal(TokenRefreshOutcome.Unavailable, result.Outcome);
    }
}
