using Host.Configuration;
using Host.Middleware;
using Host.Services;
using Microsoft.Extensions.Options;

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
        services.AddScoped<SessionGuardMiddleware>();
        services
            .AddHttpClient<KeycloakTokenClient>(
                (serviceProvider, client) =>
                {
                    // Запрос ждёт под замком сессии: держим таймаут коротким.
                    // Если за это время не успели, то по ответу идём или на ретрай, или на SignOut.
                    // Ограничен сверху RefreshThreshold проверкой на старте (OidcOptionsValidator).
                    var oidc = serviceProvider.GetRequiredService<IOptions<OidcOptions>>().Value;
                    client.Timeout = oidc.TokenRefresh.KeycloakTimeout;
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

    /// <summary>
    /// Добавляет <see cref="SessionGuardMiddleware"/> в конвейер. Размещать после UseKeycloakTokenRefresh,
    /// чтобы маркер отказа refresh уже стоял в Items.
    /// </summary>
    public static IApplicationBuilder UseSessionGuard(this IApplicationBuilder app) =>
        app.UseMiddleware<SessionGuardMiddleware>();
}
