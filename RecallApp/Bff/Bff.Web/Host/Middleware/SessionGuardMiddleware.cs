using System.Security.Claims;
using Host.Infrastructure;
using Host.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Host.Middleware;

/// <summary>
/// Граница сессии для API-запросов.
/// Анонимный запрос к /api/** получает 401 problem+json вместо редиректа в Keycloak;
/// мутации /api/** без заголовка X-CSRF отклоняются;
/// после отказа Keycloak на refresh (маркер от TokenRefreshService) /api/** отвечает
/// 401 SessionExpired, остальные пути продолжают запрос с анонимной личностью.
/// Размещать после UseAuthentication и UseKeycloakTokenRefresh.
/// </summary>
public sealed class SessionGuardMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsApiPath(context) && IsMutation(context) && !HasCsrfHeader(context))
        {
            // Кросс-сайт-запрос без preflight (форма, img) кастомный заголовок поставить не сможет.
            await ApiProblem.WriteForbiddenAsync(context, ApiProblem.CsrfFailed);
            return;
        }

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
                await ApiProblem.WriteUnauthorizedAsync(context, ApiProblem.SessionExpired);
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

    private static bool IsMutation(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method)
        || HttpMethods.IsPut(context.Request.Method)
        || HttpMethods.IsPatch(context.Request.Method)
        || HttpMethods.IsDelete(context.Request.Method);

    private static bool HasCsrfHeader(HttpContext context) =>
        string.Equals(context.Request.Headers["X-CSRF"].ToString(), "1", StringComparison.Ordinal);
}
