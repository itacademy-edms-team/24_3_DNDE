using Host.Middleware;
using Host.Services;

namespace Host.Extensions;

/// <summary>
/// Набор расширений для работы с обновлениями access-токена в Keycloak
/// </summary>
public static class TokenRefreshExtensions
{
    public static IServiceCollection AddKeycloakTokenRefreshServices(
        this IServiceCollection services
    )
    {
        services.AddSingleton<TokenRefreshSemaphores>();
        services.AddHostedService<TokenRefreshSessionJanitor>();
        services.AddScoped<ITokenRefreshService, TokenRefreshService>();
        services.AddScoped<TokenRefreshMiddleware>();
        services
            .AddHttpClient(
                TokenRefreshService.HttpClientName,
                client =>
                {
                    // Запрос ждёт под замком сессии: держим таймаут коротким.
                    // Если за 5 сек. не успели, то по ответу идём или на ретрай, или на SignOut.
                    client.Timeout = TimeSpan.FromSeconds(5);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(4),
                }
            );
        return services;
    }

    public static IApplicationBuilder UseKeycloakTokenRefresh(this IApplicationBuilder app) =>
        app.UseMiddleware<TokenRefreshMiddleware>();
}
