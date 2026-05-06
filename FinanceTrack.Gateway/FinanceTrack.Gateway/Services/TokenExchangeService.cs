using System.Collections.Concurrent;
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

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;

    public TokenExchangeService(
        HttpClient httpClient,
        IOptions<OidcOptions> oidcOptions,
        IMemoryCache cache,
        ILogger<TokenExchangeService> logger,
        TokenExchangeLocks locks
    )
    {
        _httpClient = httpClient;
        _oidcOptions = oidcOptions.Value;
        _cache = cache;
        _logger = logger;
        _semaphores = locks.Semaphores;
    }

    public async Task<TokenExchangeResult> ExchangeTokenAsync(
        string subjectToken,
        string targetAudience,
        string[]? scopes = null,
        CancellationToken cancellationToken = default
    )
    {
        // Cache key uses sub (stable across token refreshes) instead of token hash
        var sub = ExtractSub(subjectToken);
        var scopesKey = scopes != null ? string.Join(",", scopes) : "";
        var cacheKey =
            $"token_exchange:{sub ?? subjectToken.GetHashCode().ToString()}:{targetAudience}:{scopesKey}";

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

        // Prevent stampede: only one exchange per unique key at a time
        var sem = _semaphores.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring the lock
            if (_cache.TryGetValue(cacheKey, out cachedResult) && cachedResult != null)
            {
                _logger.LogDebug(
                    "Returning cached exchanged token for audience {Audience} (after lock)",
                    targetAudience
                );
                return cachedResult;
            }

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

            // Use the actual JWT exp claim for cache duration because Keycloak may return
            // expires_in = full client lifetime even when the issued token's exp is limited
            // by the subject token's remaining lifetime. Trusting expires_in would cache a
            // stale token and cause silent 401s once the 5-minute JWT ClockSkew window closes.
            var cacheExpiration = ComputeCacheDuration(tokenResponse.AccessToken, tokenResponse.ExpiresIn);
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
        finally
        {
            sem.Release();
        }
    }

    private static TimeSpan ComputeCacheDuration(string accessToken, int expiresIn)
    {
        const int bufferSeconds = 30;
        var exp = ExtractExp(accessToken);
        if (exp.HasValue)
        {
            var remaining = exp.Value - DateTimeOffset.UtcNow - TimeSpan.FromSeconds(bufferSeconds);
            return remaining > TimeSpan.FromSeconds(10) ? remaining : TimeSpan.FromSeconds(10);
        }
        return TimeSpan.FromSeconds(Math.Max(expiresIn - bufferSeconds, 10));
    }

    private static DateTimeOffset? ExtractExp(string jwtToken)
    {
        try
        {
            var parts = jwtToken.Split('.');
            if (parts.Length < 2)
                return null;
            var payload = parts[1];
            var padded = (payload.Length % 4) switch
            {
                2 => payload + "==",
                3 => payload + "=",
                _ => payload,
            };
            var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
            using var doc = JsonDocument.Parse(bytes);
            return doc.RootElement.TryGetProperty("exp", out var exp)
                ? DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64())
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractSub(string jwtToken)
    {
        try
        {
            var payload = jwtToken.Split('.')[1];
            var padded = (payload.Length % 4) switch
            {
                2 => payload + "==",
                3 => payload + "=",
                _ => payload,
            };
            var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
            using var doc = JsonDocument.Parse(bytes);
            return doc.RootElement.TryGetProperty("sub", out var sub) ? sub.GetString() : null;
        }
        catch
        {
            return null;
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
