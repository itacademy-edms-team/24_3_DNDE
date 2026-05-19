using FinanceTrack.Gateway.Configuration;
using FinanceTrack.Gateway.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Model;

namespace FinanceTrack.Gateway.Middleware;

// Используется в 1-й фазе токен-обмена, до отправки запроса к downstream API.
// Получает новый токен и сохраняет его в HttpContext.Items["bff_exchanged_token"] для дальнейшего использования в трансформации запроса.
public sealed class TokenExchangeMiddleware(
    ITokenExchangeService tokenExchangeService,
    IOptions<OidcOptions> oidcOptions,
    ILogger<TokenExchangeMiddleware> logger
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await TryExchangeTokenAsync(context);
        await next(context);
    }

    private async Task TryExchangeTokenAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        var options = oidcOptions.Value;
        if (!options.TokenExchange.Enabled)
            return;

        var routeModel = context.GetEndpoint()?.Metadata.GetMetadata<RouteModel>();
        if (routeModel is null)
            return;

        // Получить конфигурацию токен-обмена для данного маршрута. Если нету - пропускаем.
        var serviceConfig = options.TokenExchange.GetByRouteId(routeModel.Config.RouteId);
        if (serviceConfig is null || string.IsNullOrEmpty(serviceConfig.Audience))
            return;

        var accessToken =
            context.Items.TryGetValue("bff_access_token", out var tokenObj) && tokenObj is string t
                ? t
                : await context.GetTokenAsync("access_token");

        if (string.IsNullOrEmpty(accessToken))
            return;

        var result = await tokenExchangeService.ExchangeTokenAsync(
            accessToken,
            serviceConfig.Audience,
            serviceConfig.Scopes,
            context.RequestAborted
        );

        if (result.Success && !string.IsNullOrEmpty(result.AccessToken))
        {
            context.Items["bff_exchanged_token"] = result.AccessToken;
            logger.LogDebug(
                "Token exchanged for route {RouteId} (audience: {Audience})",
                routeModel.Config.RouteId,
                serviceConfig.Audience
            );
        }
        else
        {
            logger.LogWarning(
                "Token exchange failed for route {RouteId}: {Error} - {Description}. Using original token.",
                routeModel.Config.RouteId,
                result.Error,
                result.ErrorDescription
            );
            context.Items["bff_exchanged_token"] = accessToken;
        }
    }
}
