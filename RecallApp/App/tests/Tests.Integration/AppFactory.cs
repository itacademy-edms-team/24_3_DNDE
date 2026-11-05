using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Tests.Integration;

/// <summary>
/// Фабрика тестового хоста: единая точка настройки App для интеграционных тестов.
/// Аутентификация - тестовая схема (claims без JWT и Keycloak), challenge остаётся на JwtBearer.
/// </summary>
public sealed class AppFactory : WebApplicationFactory<Program>
{
    public const string TestAuthScheme = "TestAuth";
    public const string TestAuthHeader = "X-Test-Auth";

    /// <summary>Подмена claim sub: проверка отказов по невалидной идентичности.</summary>
    public const string TestSubHeader = "X-Test-Sub";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Env-first: в контейнере (app-tests) строки подключения и authority приходят из compose,
        // локально подменяем заглушками, чтобы хост стартовал без БД.
        if (Environment.GetEnvironmentVariable("ConnectionStrings__Database") is null)
        {
            builder = builder
                .UseSetting("ConnectionStrings:Database", "Host=localhost;Database=unused")
                .UseSetting("ConnectionStrings:AccountsMigrations", "Host=localhost;Database=unused")
                .UseSetting("Oidc:Authority", "http://localhost/realms/recallapp")
                .UseSetting("Oidc:RequireHttpsMetadata", "false");
        }

        builder
            .UseEnvironment("Development")
            .ConfigureServices(services => services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthScheme;
                    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthScheme, _ => { }));
    }
}

/// <summary>
/// Аутентифицирует запрос фиксированным набором claims при наличии заголовка X-Test-Auth.
/// </summary>
internal sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, UrlEncoder.Default)
{
    public static readonly Claim[] Claims =
    [
        new("sub", "11111111-2222-3333-4444-555555555555"),
        new("email", "testuser@example.com"),
        new("email_verified", "true"),
        new("name", "Test User"),
    ];

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue(AppFactory.TestAuthHeader, out var value) || value != "true")
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = Claims.AsEnumerable();
        if (Context.Request.Headers.TryGetValue(AppFactory.TestSubHeader, out var sub))
            claims = claims.Where(c => c.Type != "sub").Append(new Claim("sub", sub.ToString()));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
