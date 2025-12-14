using System.Net.Http.Headers;
using FinanceTrack.Gateway.Configuration;
using FinanceTrack.Gateway.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Transforms;

namespace FinanceTrack.Gateway.Extensions;

/// <summary>
/// Extension methods for adding Token Exchange transforms to YARP reverse proxy
/// </summary>
public static class ReverseProxyTokenExchangeExtensions
{
    /// <summary>
    /// Adds token exchange transform that automatically exchanges tokens for configured routes.
    /// Tokens are exchanged based on the TokenExchange configuration in OidcOptions.
    /// </summary>
    public static IReverseProxyBuilder AddTokenExchangeTransform(this IReverseProxyBuilder builder)
    {
        return builder.AddTransforms(builderContext =>
        {
            var routeId = builderContext.Route.RouteId;

            builderContext.AddRequestTransform(async transformContext =>
            {
                var httpContext = transformContext.HttpContext;

                var accessToken = await GetAccessTokenAsync(httpContext);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                accessToken = await TryExchangeTokenAsync(httpContext, routeId, accessToken);

                transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken
                );
            });
        });
    }

    private static async Task<string?> GetAccessTokenAsync(HttpContext httpContext)
    {
        // Check if token was refreshed by middleware
        if (
            httpContext.Items.TryGetValue("bff_access_token", out var tokenObj)
            && tokenObj is string token
        )
        {
            return token;
        }

        // Otherwise get from auth properties
        return await httpContext.GetTokenAsync("access_token");
    }

    private static async Task<string> TryExchangeTokenAsync(
        HttpContext httpContext,
        string routeId,
        string accessToken
    )
    {
        var oidcOptions = httpContext
            .RequestServices.GetRequiredService<IOptions<OidcOptions>>()
            .Value;

        if (!oidcOptions.TokenExchange.Enabled)
        {
            return accessToken;
        }

        var serviceConfig = oidcOptions.TokenExchange.GetByRouteId(routeId);
        if (serviceConfig == null || string.IsNullOrEmpty(serviceConfig.Audience))
        {
            return accessToken;
        }

        var tokenExchangeService =
            httpContext.RequestServices.GetRequiredService<ITokenExchangeService>();

        var exchangeResult = await tokenExchangeService.ExchangeTokenAsync(
            accessToken,
            serviceConfig.Audience,
            serviceConfig.Scopes,
            httpContext.RequestAborted
        );

        var logger = httpContext.RequestServices.GetRequiredService<
            ILogger<TokenExchangeService>
        >();

        if (exchangeResult.Success && !string.IsNullOrEmpty(exchangeResult.AccessToken))
        {
            logger.LogDebug(
                "Token exchanged for route {RouteId} (audience: {Audience})",
                routeId,
                serviceConfig.Audience
            );
            return exchangeResult.AccessToken;
        }

        logger.LogWarning(
            "Token exchange failed for route {RouteId}: {Error} - {Description}. Using original token.",
            routeId,
            exchangeResult.Error,
            exchangeResult.ErrorDescription
        );

        return accessToken;
    }
}
