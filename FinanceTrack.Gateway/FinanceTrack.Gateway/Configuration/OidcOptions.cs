namespace FinanceTrack.Gateway.Configuration;

public class BffOptions
{
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Configuration for a downstream service in token exchange
/// </summary>
public class DownstreamServiceOptions
{
    /// <summary>
    /// Target audience (client_id in Keycloak) for token exchange
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Route ID in YARP configuration (e.g., "finance-api")
    /// </summary>
    public string RouteId { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Additional scopes to request during token exchange
    /// </summary>
    public string[] Scopes { get; set; } = [];
}

public class TokenLifetimeOptions
{
    /// <summary>
    /// How long before token expiration to attempt a refresh.
    /// Token refresh happens when time until token expiry is less than this threshold.
    /// </summary>
    public TimeSpan RefreshThreshold { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Buffer time expecting servers clock desynchronization issues.
    /// This buffer is subtracted from the token's lifetime when calculating the expires_at value stored in the cookie and when determining the TTL for caching exchange tokens.
    /// </summary>
    public TimeSpan ClockSkewBuffer { get; set; } = TimeSpan.FromSeconds(30);
}

public class TokenExchangeOptions
{
    /// <summary>
    /// Enable token exchange for downstream API calls
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Downstream services configuration
    /// Key: Service name (e.g., "FinanceTrack.Finance")
    /// </summary>
    public Dictionary<string, DownstreamServiceOptions> Services { get; set; } = new();

    /// <summary>
    /// Get service options by route ID
    /// </summary>
    public DownstreamServiceOptions? GetByRouteId(string routeId)
    {
        return Services.Values.FirstOrDefault(s =>
            s.RouteId.Equals(routeId, StringComparison.OrdinalIgnoreCase)
        );
    }
}

public class OidcOptions
{
    public const string SectionName = "Oidc";

    public BffOptions Bff { get; set; } = new();
    public TokenLifetimeOptions TokenLifetime { get; set; } = new();
    public TokenExchangeOptions TokenExchange { get; set; } = new();
}
