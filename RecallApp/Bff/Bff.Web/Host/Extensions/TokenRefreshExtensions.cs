using Host.Middleware;
using Host.Services;

namespace Host.Extensions;

/// <summary>
/// Расширения для регистрация сервисов и middleware упреждающего обновления access-токена.
/// </summary>
public static class TokenRefreshExtensions
{
    /// <summary>
    /// Регистрирует состояние замков, уборщика, писателя сессии, координатор refresh
    /// и typed HttpClient <see cref="KeycloakTokenClient"/> с коротким таймаутом.
    /// </summary>
    public static IServiceCollection AddKeycloakTokenRefreshServices(
        this IServiceCollection services
    )
    {
        services.AddSingleton<TokenRefreshSemaphores>();
        services.AddHostedService<TokenRefreshSessionJanitor>();
        services.AddSingleton<SessionTokenWriter>();
        services.AddScoped<ITokenRefreshService, TokenRefreshService>();
        services.AddScoped<TokenRefreshMiddleware>();
        services
            .AddHttpClient<KeycloakTokenClient>(
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

    /// <summary>Добавляет <see cref="TokenRefreshMiddleware"/> в конвейер. Размещать после UseAuthentication.</summary>
    public static IApplicationBuilder UseKeycloakTokenRefresh(this IApplicationBuilder app) =>
        app.UseMiddleware<TokenRefreshMiddleware>();
}
