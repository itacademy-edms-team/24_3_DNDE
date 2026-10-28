using Host.Configuration;
using Host.Infrastructure;
using Microsoft.AspNetCore.Authentication;

namespace Tests.Unit;

public class SessionCookieLifetimeTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private readonly SsoSessionOptions _sso = new();

    [Fact]
    public void Apply_RememberMeOff_SetsSessionCookieAndRegularIdleWindow()
    {
        var properties = new AuthenticationProperties();

        SessionCookieLifetime.Apply(properties, rememberMe: false, _sso, Now);

        Assert.False(properties.IsPersistent);
        Assert.False(SessionCookieLifetime.ReadRememberMe(properties));
        Assert.Equal(Now, properties.IssuedUtc);
        Assert.Equal(Now + _sso.SessionIdle, properties.ExpiresUtc);
    }

    [Fact]
    public void Apply_RememberMeOn_SetsPersistentCookieAndRememberMeIdleWindow()
    {
        var properties = new AuthenticationProperties();

        SessionCookieLifetime.Apply(properties, rememberMe: true, _sso, Now);

        Assert.True(properties.IsPersistent);
        Assert.True(SessionCookieLifetime.ReadRememberMe(properties));
        Assert.Equal(Now + _sso.SessionIdleRememberMe, properties.ExpiresUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResetWindow_AfterApply_RestartsWindowInLoginMode(bool rememberMe)
    {
        var properties = new AuthenticationProperties();
        SessionCookieLifetime.Apply(properties, rememberMe, _sso, Now);
        var later = Now.AddMinutes(10);

        SessionCookieLifetime.ResetWindow(properties, _sso, later);

        Assert.Equal(later, properties.IssuedUtc);
        Assert.Equal(later + SessionCookieLifetime.ResolveIdle(rememberMe, _sso), properties.ExpiresUtc);
        Assert.Equal(rememberMe, properties.IsPersistent);
    }

    [Fact]
    public void ReadRememberMe_NoMarker_ReturnsFalse()
    {
        Assert.False(SessionCookieLifetime.ReadRememberMe(new AuthenticationProperties()));
    }
}
