using FinanceTrack.Gateway.Middleware;
using FinanceTrack.Gateway.Services;

namespace FinanceTrack.Gateway.Extensions;

public static class TokenRefreshExtensions
{
    public static IServiceCollection AddKeycloakTokenRefreshServices(
        this IServiceCollection services
    )
    {
        services.AddSingleton<TokenRefreshSemaphores>();
        services.AddScoped<ITokenRefreshService, TokenRefreshService>();
        services.AddScoped<TokenRefreshMiddleware>();
        services
            .AddHttpClient(
                TokenRefreshService.HttpClientName,
                client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
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
