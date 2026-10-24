using Host.Services;

namespace Host.Middleware;

/// <summary>
/// Middleware для обновления access-токена из Keycloak.
/// На каждый не-WebSocket запрос запускает упреждающее обновление токена до передачи управления дальше.
/// Должен размещаться после UseAuthentication (нужен аутентифицированный principal и сохранённые токены).
/// </summary>
public sealed class TokenRefreshMiddleware(ITokenRefreshService refreshService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.WebSockets.IsWebSocketRequest)
            await refreshService.TryRefreshTokenAsync(context).ConfigureAwait(false);
        await next(context).ConfigureAwait(false);
    }
}
