using System.Security.Claims;
using Host.Configuration;
using Host.Infrastructure;
using Host.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tests.Integration.Helpers;

/// <summary>Сессия, которую тест засевает в обход OIDC-входа.</summary>
/// <param name="WithoutSessionId">Не ставить bff_session_id: сессия без ключа сериализации refresh.</param>
public sealed record TestSession(
    string AccessToken,
    string? RefreshToken = "initial-refresh",
    TimeSpan? AccessLifetime = null,
    bool RememberMe = false,
    bool WithoutSessionId = false
);

/// <summary>Состояние сессии, как его видит cookie-схема (без TokenRefreshMiddleware).</summary>
public sealed record SessionSnapshot(
    bool IsAuthenticated,
    Dictionary<string, string> Tokens,
    DateTimeOffset? IssuedUtc,
    DateTimeOffset? ExpiresUtc,
    bool IsPersistent
);

/// <summary>
/// Тестовые endpoint-ы, подключаемые перед конвейером приложения только в интеграционных тестах.
/// POST /test/signin создаёт сессию так же, как вход (SessionCookieLifetime.Apply, bff_session_id),
/// GET /test/session отдаёт её содержимое,
/// GET /api/echo имитирует защищённый API за BFF.
/// </summary>
public sealed class TestSessionEndpoints : IStartupFilter
{
    public const string SignInPath = "/test/signin";
    public const string SessionPath = "/test/session";
    public const string ApiEchoPath = "/api/echo";
    public const string ApiThrowPath = "/api/throw";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            // MapWhen, а не Map: Map переносит путь в PathBase, и cookie-схема выставила бы cookie
            // с path=/test/signin, невидимую для остального приложения.
            app.MapWhen(c => c.Request.Path == SignInPath, branch => branch.Run(SignInAsync));
            app.MapWhen(c => c.Request.Path == SessionPath, branch => branch.Run(ReadSessionAsync));
            // За конвейером: echo обязан пройти аутентификацию, refresh и SessionGuardMiddleware,
            // как будущие API за YARP. До next(app) он бы обслуживался в обход всего pipeline.
            next(app);
            app.MapWhen(c => c.Request.Path == ApiEchoPath, branch => branch.Run(ApiEchoAsync));
            app.MapWhen(c => c.Request.Path == ApiThrowPath, branch => branch.Run(ApiThrowAsync));
        };

    private static async Task SignInAsync(HttpContext context)
    {
        var session = await context.Request.ReadFromJsonAsync<TestSession>();
        var oidc = context.RequestServices.GetRequiredService<IOptions<OidcOptions>>().Value;
        var now = context.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();

        var properties = new AuthenticationProperties();
        if (!session!.WithoutSessionId)
            properties.SetString(BffSessionKeys.SessionId, Guid.NewGuid().ToString("N"));
        SessionCookieLifetime.Apply(properties, session.RememberMe, oidc.SsoSession, now);

        var tokens = new List<AuthenticationToken>
        {
            new() { Name = "access_token", Value = session.AccessToken },
            new() { Name = "id_token", Value = "initial-id" },
            new() { Name = "token_type", Value = "Bearer" },
            new()
            {
                Name = AccessTokenExpiryHelper.TokenName,
                Value = AccessTokenExpiryHelper.Format(
                    AccessTokenExpiryHelper.Effective(
                        now + (session.AccessLifetime ?? TimeSpan.FromMinutes(5)),
                        oidc.TokenRefresh.ClockSkewBuffer
                    )
                ),
            },
        };
        if (session.RefreshToken is not null)
            tokens.Add(new() { Name = "refresh_token", Value = session.RefreshToken });
        properties.StoreTokens(tokens);

        var identity = new ClaimsIdentity(
            [
                new Claim("sub", "user-1"),
                new Claim("preferred_username", "testuser"),
                new Claim("name", "Test User"),
            ],
            "Test",
            "name",
            "roles"
        );
        await context
            .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);
    }

    private static async Task ApiEchoAsync(HttpContext context)
    {
        await context.Response.WriteAsJsonAsync(
            new { authenticated = context.User.Identity?.IsAuthenticated == true }
        );
    }

    private static Task ApiThrowAsync(HttpContext context) =>
        throw new InvalidOperationException("test exception marker");

    private static async Task ReadSessionAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = result.Properties;
        await context
            .Response.WriteAsJsonAsync(
                new SessionSnapshot(
                    result.Succeeded,
                    properties?.GetTokens().ToDictionary(t => t.Name, t => t.Value) ?? [],
                    properties?.IssuedUtc,
                    properties?.ExpiresUtc,
                    properties?.IsPersistent ?? false
                )
            );
    }
}
