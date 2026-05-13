using System.Net.Http.Headers;
using FinanceTrack.Gateway.Middleware;
using FinanceTrack.Gateway.Services;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace FinanceTrack.Gateway.Extensions;

public static class TokenExchangeExtensions
{
    public static IServiceCollection AddKeycloakTokenExchangeServices(
        this IServiceCollection services
    )
    {
        services.AddMemoryCache();
        services.AddSingleton<TokenExchangeSemaphores>();
        services.AddSingleton<ITransformProvider, TokenExchangeTransformProvider>();

        services.AddScoped<TokenExchangeMiddleware>();
        services
            .AddHttpClient<ITokenExchangeService, TokenExchangeService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(4),
                }
            );
        return services;
    }

    public static IApplicationBuilder UseKeycloakTokenExchange(this IApplicationBuilder app) =>
        app.UseMiddleware<TokenExchangeMiddleware>();
}

// Используется во 2-й фазе токен-обмена, после получения нового токена и сохранения его в HttpContext.Items["bff_exchanged_token"].
internal sealed class TokenExchangeTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }

    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;
            if (
                httpContext.Items.TryGetValue("bff_exchanged_token", out var tokenObj)
                && tokenObj is string token
                && !string.IsNullOrEmpty(token)
            )
            {
                transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }
            return ValueTask.CompletedTask;
        });
    }
}
