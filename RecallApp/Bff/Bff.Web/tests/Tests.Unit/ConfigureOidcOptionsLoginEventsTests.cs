using System.Globalization;
using System.Security.Claims;
using Host.Configuration;
using Host.Infrastructure;
using Host.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary>
/// События входа, которые регистрирует <see cref="ConfigureOidcOptions"/>.
/// Тест собирает OpenIdConnectOptions настоящим Configure и вызывает зарегистрированные события,
/// поэтому проверяется и логика, и её подключение к OIDC-handler-у.
/// </summary>
public class ConfigureOidcOptionsLoginEventsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly AuthenticationScheme Scheme = new(
        OpenIdConnectDefaults.AuthenticationScheme,
        displayName: null,
        typeof(OpenIdConnectHandler)
    );

    private readonly OidcOptions _oidc = TestOidcOptions.Create();
    private readonly OpenIdConnectOptions _options = new();

    public ConfigureOidcOptionsLoginEventsTests()
    {
        new ConfigureOidcOptions(TestOidcOptions.Wrap(_oidc), new FakeTimeProvider(Now)).Configure(
            OpenIdConnectDefaults.AuthenticationScheme,
            _options
        );
    }

    private async Task<AuthenticationProperties> RunTokenValidatedAsync(
        OpenIdConnectMessage? tokenEndpointResponse
    )
    {
        var context = new TokenValidatedContext(
            new DefaultHttpContext(),
            Scheme,
            _options,
            new ClaimsPrincipal(new ClaimsIdentity("Test")),
            new AuthenticationProperties()
        )
        {
            TokenEndpointResponse = tokenEndpointResponse,
        };

        await _options.Events.TokenValidated(context);
        return context.Properties!;
    }

    private static OpenIdConnectMessage ResponseWithRefreshExpiresIn(string value) =>
        new() { Parameters = { ["refresh_expires_in"] = value } };

    private async Task<AuthenticationProperties> RunTicketReceivedAsync(
        AuthenticationProperties properties
    )
    {
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity("Test")),
            properties,
            OpenIdConnectDefaults.AuthenticationScheme
        );
        var context = new TicketReceivedContext(new DefaultHttpContext(), Scheme, _options, ticket);

        await _options.Events.TicketReceived(context);
        return context.Properties!;
    }

    [Fact]
    public async Task OnTokenValidated_AnyLogin_SetsNewSessionIdPerLogin()
    {
        var first = await RunTokenValidatedAsync(ResponseWithRefreshExpiresIn("1800"));
        var second = await RunTokenValidatedAsync(ResponseWithRefreshExpiresIn("1800"));

        var firstId = first.GetString(BffSessionKeys.SessionId);
        Assert.Matches("^[0-9a-f]{32}$", firstId);
        Assert.NotEqual(firstId, second.GetString(BffSessionKeys.SessionId));
    }

    [Fact]
    public async Task OnTokenValidated_RefreshLifetimeAboveThreshold_AppliesRememberMeMode()
    {
        // 30 дней: refresh-токен сессии Remember Me.
        var properties = await RunTokenValidatedAsync(ResponseWithRefreshExpiresIn("2592000"));

        Assert.True(properties.IsPersistent);
        Assert.True(SessionCookieLifetime.ReadRememberMe(properties));
        Assert.Equal(Now, properties.IssuedUtc);
        Assert.Equal(Now + _oidc.SsoSession.SessionIdleRememberMe, properties.ExpiresUtc);
    }

    [Fact]
    public async Task OnTokenValidated_RefreshLifetimeBelowThreshold_AppliesRegularMode()
    {
        // 30 минут: обычная сессия.
        var properties = await RunTokenValidatedAsync(ResponseWithRefreshExpiresIn("1800"));

        Assert.False(properties.IsPersistent);
        Assert.False(SessionCookieLifetime.ReadRememberMe(properties));
        Assert.Equal(Now, properties.IssuedUtc);
        Assert.Equal(Now + _oidc.SsoSession.SessionIdle, properties.ExpiresUtc);
    }

    // Граничное значение: ровно порог ещё обычной сессии, Remember Me - строго больше.
    [Fact]
    public async Task OnTokenValidated_RefreshLifetimeEqualToThreshold_AppliesRegularMode()
    {
        var threshold = (long)_oidc.TokenRefresh.RememberMeThreshold.TotalSeconds;

        var properties = await RunTokenValidatedAsync(ResponseWithRefreshExpiresIn(threshold.ToString(CultureInfo.InvariantCulture)));

        Assert.False(SessionCookieLifetime.ReadRememberMe(properties));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    public async Task OnTokenValidated_InvalidRefreshLifetime_AppliesRegularMode(string value)
    {
        var properties = await RunTokenValidatedAsync(ResponseWithRefreshExpiresIn(value));

        Assert.False(SessionCookieLifetime.ReadRememberMe(properties));
        Assert.Equal(Now + _oidc.SsoSession.SessionIdle, properties.ExpiresUtc);
    }

    [Fact]
    public async Task OnTokenValidated_ResponseWithoutRefreshLifetime_AppliesRegularMode()
    {
        var properties = await RunTokenValidatedAsync(new OpenIdConnectMessage());

        Assert.False(SessionCookieLifetime.ReadRememberMe(properties));
    }

    [Fact]
    public async Task OnTokenValidated_NoTokenEndpointResponse_AppliesRegularMode()
    {
        var properties = await RunTokenValidatedAsync(tokenEndpointResponse: null);

        Assert.False(SessionCookieLifetime.ReadRememberMe(properties));
        Assert.NotNull(properties.GetString(BffSessionKeys.SessionId));
    }

    [Fact]
    public async Task OnTicketReceived_ExpiresAtFromSaveTokens_SubtractsClockSkewBuffer()
    {
        var savedExpiry = Now.AddMinutes(5);
        var properties = new AuthenticationProperties();
        properties.StoreTokens(
            [
                new() { Name = "access_token", Value = "access" },
                new() { Name = "token_type", Value = "Bearer" },
                new() { Name = "expires_at", Value = AccessTokenExpiryHelper.Format(savedExpiry) },
            ]
        );

        var result = await RunTicketReceivedAsync(properties);

        Assert.Equal(
            AccessTokenExpiryHelper.Format(savedExpiry - _oidc.TokenRefresh.ClockSkewBuffer),
            result.GetTokenValue("expires_at")
        );
        Assert.Equal("access", result.GetTokenValue("access_token"));
        Assert.Equal("Bearer", result.GetTokenValue("token_type"));
    }

    [Fact]
    public async Task OnTicketReceived_NoExpiresAt_LeavesTokensUnchanged()
    {
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new() { Name = "access_token", Value = "access" }]);

        var result = await RunTicketReceivedAsync(properties);

        Assert.Null(result.GetTokenValue("expires_at"));
        Assert.Equal("access", result.GetTokenValue("access_token"));
    }

    [Fact]
    public async Task OnTicketReceived_UnparsableExpiresAt_LeavesValueUnchanged()
    {
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new() { Name = "expires_at", Value = "garbage" }]);

        var result = await RunTicketReceivedAsync(properties);

        Assert.Equal("garbage", result.GetTokenValue("expires_at"));
    }

    [Fact]
    public void Configure_OtherSchemeName_LeavesOptionsUntouched()
    {
        var options = new OpenIdConnectOptions();

        new ConfigureOidcOptions(TestOidcOptions.Wrap(_oidc), new FakeTimeProvider(Now)).Configure(
            "Other",
            options
        );

        Assert.Null(options.Authority);
        Assert.Null(options.ClientId);
    }
}
