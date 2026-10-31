using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Host.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Tests.Integration.Helpers;

/// <summary>
/// BFF в процессе теста: настоящий конвейер,
/// вместо Keycloak <see cref="FakeKeycloak"/>, вместо системных часов <see cref="FakeTimeProvider"/>.
/// Один экземпляр на тест: часы и замки сессий у каждого теста свои.
/// </summary>
public sealed class BffApp : WebApplicationFactory<Program>
{
    // Часы стартуют от реального «сейчас»: MemoryDistributedCache живёт по системным часам,
    // и записи стора не должны истечь раньше времени. Старт с целой секунды, т.к. cookie-схема
    // сериализует IssuedUtc/ExpiresUtc с точностью до секунды.
    public FakeTimeProvider Time { get; } =
        new(DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

    public FakeKeycloak Keycloak { get; } = new();

    /// <summary>Окно обычной сессии (SSO Idle). По умолчанию как в ADR 0027.</summary>
    public TimeSpan SessionIdle { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Настоящий Keycloak вместо <see cref="FakeKeycloak"/>: BFF ходит в тестовый realm
    /// как клиент из настроек. Часы BFF при этом остаются поддельными.
    /// </summary>
    public KeycloakTestSettings? RealKeycloak { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Oidc:Authority", RealKeycloak?.Authority ?? "https://auth.test/realms/recallapp");
        builder.UseSetting("Oidc:ClientId", RealKeycloak?.ClientId ?? "Bff.Web");
        builder.UseSetting("Oidc:ClientSecret", RealKeycloak?.ClientSecret ?? "test-secret");
        builder.UseSetting("Oidc:SsoSession:SessionIdle", SessionIdle.ToString("c", CultureInfo.InvariantCulture));

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TimeProvider>(Time);
            if (RealKeycloak is null)
            {
                services.AddHttpClient<KeycloakTokenClient>().ConfigurePrimaryHttpMessageHandler(() => Keycloak);
                // Challenge-тесты (401 для /api/**, 302 для /login) не должны ходить за discovery
                // к несуществующему issuer: статическая конфигурация его endpoint-ов.
                services.PostConfigure<OpenIdConnectOptions>(
                    OpenIdConnectDefaults.AuthenticationScheme,
                    options =>
                    {
                        const string authority = "https://auth.test/realms/recallapp";
                        options.Configuration = new OpenIdConnectConfiguration
                        {
                            AuthorizationEndpoint = $"{authority}/protocol/openid-connect/auth",
                            TokenEndpoint = $"{authority}/protocol/openid-connect/token",
                            EndSessionEndpoint = $"{authority}/protocol/openid-connect/logout",
                        };
                        options.ConfigurationManager = new StaticConfigurationManager<
                            OpenIdConnectConfiguration
                        >(options.Configuration);
                    }
                );
            }
            services.AddSingleton<IStartupFilter, TestSessionEndpoints>();
        });
    }

    /// <summary>Клиент с cookie-контейнером, без автоматических редиректов.</summary>
    public HttpClient CreateBrowser() =>
        CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
                HandleCookies = true,
            }
        );

    /// <summary>Засеять сессию в браузер; возвращает ответ с Set-Cookie входа.</summary>
    public static async Task<HttpResponseMessage> SignInAsync(HttpClient browser, TestSession session)
    {
        var response = await browser.PostAsJsonAsync(TestSessionEndpoints.SignInPath, session);
        response.EnsureSuccessStatusCode();
        return response;
    }

    public static async Task<SessionSnapshot> ReadSessionAsync(HttpClient browser) =>
        (await browser.GetFromJsonAsync<SessionSnapshot>(TestSessionEndpoints.SessionPath))!;

    /// <summary>GET /session приложения; возвращает ответ и разобранное тело.</summary>
    public static async Task<(HttpResponseMessage Response, JsonElement Body)> GetAppSessionAsync(
        HttpClient browser,
        string path = "/session"
    )
    {
        var response = await browser.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (response, body);
    }

    /// <summary>Заголовки Set-Cookie для cookie сессии recall_bff.</summary>
    public static string[] SessionCookies(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.Where(v => v.StartsWith("recall_bff=", StringComparison.Ordinal)).ToArray()
            : [];
}
