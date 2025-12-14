using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceTrack.Gateway.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FinanceTrack.Gateway.Services;

/// <summary>
/// Service for performing OAuth 2.0 Token Exchange (RFC 8693)
/// Exchanges subject tokens for tokens with specific audiences
/// </summary>
public interface ITokenExchangeService
{
    /// <summary>
    /// Exchange a subject token for a new token with specified audience
    /// </summary>
    /// <param name="subjectToken">The access token to exchange</param>
    /// <param name="targetAudience">The target client_id (audience)</param>
    /// <param name="scopes">Optional additional scopes to request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token for the target audience</returns>
    Task<TokenExchangeResult> ExchangeTokenAsync(
        string subjectToken,
        string targetAudience,
        string[]? scopes = null,
        CancellationToken cancellationToken = default
    );
}

public record TokenExchangeResult(
    bool Success,
    string? AccessToken,
    int ExpiresIn,
    string? Error,
    string? ErrorDescription
);

public class TokenExchangeService : ITokenExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly OidcOptions _oidcOptions;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenExchangeService> _logger;

    private const string TokenExchangeGrantType = "urn:ietf:params:oauth:grant-type:token-exchange";
    private const string AccessTokenType = "urn:ietf:params:oauth:token-type:access_token";

    public TokenExchangeService(
        HttpClient httpClient,
        IOptions<OidcOptions> oidcOptions,
        IMemoryCache cache,
        ILogger<TokenExchangeService> logger
    )
    {
        _httpClient = httpClient;
        _oidcOptions = oidcOptions.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TokenExchangeResult> ExchangeTokenAsync(
        string subjectToken,
        string targetAudience,
        string[]? scopes = null,
        CancellationToken cancellationToken = default
    )
    {
        // Create cache key based on subject token hash, target audience and scopes
        var scopesKey = scopes != null ? string.Join(",", scopes) : "";
        var cacheKey = $"token_exchange:{subjectToken.GetHashCode()}:{targetAudience}:{scopesKey}";

        // Check cache first
        if (
            _cache.TryGetValue(cacheKey, out TokenExchangeResult? cachedResult)
            && cachedResult != null
        )
        {
            _logger.LogDebug(
                "Returning cached exchanged token for audience {Audience}",
                targetAudience
            );
            return cachedResult;
        }

        try
        {
            var tokenEndpoint = $"{_oidcOptions.Bff.Authority}/protocol/openid-connect/token";

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = TokenExchangeGrantType,
                ["subject_token"] = subjectToken,
                ["subject_token_type"] = AccessTokenType,
                ["requested_token_type"] = AccessTokenType,
                ["audience"] = targetAudience,
                ["client_id"] = _oidcOptions.Bff.ClientId,
                ["client_secret"] = _oidcOptions.Bff.ClientSecret,
            };

            // Add scopes if specified
            if (scopes is { Length: > 0 })
            {
                requestBody["scope"] = string.Join(" ", scopes);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody),
            };

            _logger.LogDebug(
                "Performing token exchange for audience {Audience} at {Endpoint}",
                targetAudience,
                tokenEndpoint
            );

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Token exchange failed with status {StatusCode}: {Response}",
                    response.StatusCode,
                    responseContent
                );

                var errorResponse = JsonSerializer.Deserialize<TokenExchangeErrorResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return new TokenExchangeResult(
                    Success: false,
                    AccessToken: null,
                    ExpiresIn: 0,
                    Error: errorResponse?.Error ?? "token_exchange_failed",
                    ErrorDescription: errorResponse?.ErrorDescription ?? responseContent
                );
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenExchangeResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (tokenResponse?.AccessToken == null)
            {
                return new TokenExchangeResult(
                    Success: false,
                    AccessToken: null,
                    ExpiresIn: 0,
                    Error: "invalid_response",
                    ErrorDescription: "Token exchange response did not contain access_token"
                );
            }

            var result = new TokenExchangeResult(
                Success: true,
                AccessToken: tokenResponse.AccessToken,
                ExpiresIn: tokenResponse.ExpiresIn,
                Error: null,
                ErrorDescription: null
            );

            // Cache the result (expire slightly before the token expires)
            var cacheExpiration = TimeSpan.FromSeconds(Math.Max(tokenResponse.ExpiresIn - 30, 10));
            _cache.Set(cacheKey, result, cacheExpiration);

            _logger.LogDebug(
                "Token exchange successful for audience {Audience}, expires in {ExpiresIn}s",
                targetAudience,
                tokenResponse.ExpiresIn
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange failed with exception");
            return new TokenExchangeResult(
                Success: false,
                AccessToken: null,
                ExpiresIn: 0,
                Error: "exception",
                ErrorDescription: ex.Message
            );
        }
    }

    private sealed record TokenExchangeResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    );

    private sealed record TokenExchangeErrorResponse(
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("error_description")] string? ErrorDescription
    );
}
