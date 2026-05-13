using FinanceTrack.Gateway.Services;

namespace FinanceTrack.Gateway.Middleware;

public sealed class TokenRefreshMiddleware(ITokenRefreshService refreshService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.WebSockets.IsWebSocketRequest)
            await refreshService.TryRefreshTokenAsync(context);
        await next(context);
    }
}
