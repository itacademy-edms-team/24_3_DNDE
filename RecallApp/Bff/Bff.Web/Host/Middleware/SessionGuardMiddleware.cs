using System.Security.Claims;
using Host.Infrastructure;
using Host.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Host.Middleware;

/// <summary>
/// Граница сессии для API-запросов.
/// Анонимный запрос к /api/** получает 401 problem+json вместо редиректа в Keycloak;
/// после отказа Keycloak на refresh (маркер от TokenRefreshService) /api/** отвечает
/// 401 session_expired, остальные пути продолжают запрос с анонимной личностью.
/// Размещать после UseAuthentication и UseKeycloakTokenRefresh.
/// </summary>
public sealed class SessionGuardMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsApiPath(context) && context.User.Identity?.IsAuthenticated != true)
        {
            // Challenge уходит в DefaultChallengeScheme (OIDC); событие OnRedirectToIdentityProvider
            // превращает редирект в 401 для /api/**.
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return;
        }

        if (context.Items.ContainsKey(BffRequestItems.SessionRejected))
        {
            if (IsApiPath(context))
            {
                // Set-Cookie с удалением сессии уже записан sign-out-ом в TokenRefreshService.
                await ApiProblem.WriteUnauthorizedAsync(context.Response, ApiProblem.SessionExpired);
                return;
            }

            // Sign-out отстаёт на один запрос: личность этого запроса уже недействительна,
            // сбрасываем её, чтобы контроллеры не работали с протухшей сессией.
            context.User = new ClaimsPrincipal(new ClaimsIdentity());
        }

        await next(context);
    }

    private static bool IsApiPath(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api");
}
