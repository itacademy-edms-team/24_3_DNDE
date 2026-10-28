using Host.Services;

namespace Tests.Unit;

public class AccessTokenExpiryHelperTests
{
    private static readonly DateTimeOffset Expiry = new(2026, 10, 28, 13, 32, 0, 843, TimeSpan.Zero);

    [Fact]
    public void Effective_ClockSkewBuffer_ReturnsExpiryMinusBuffer()
    {
        var effective = AccessTokenExpiryHelper.Effective(Expiry, TimeSpan.FromSeconds(30));

        Assert.Equal(Expiry.AddSeconds(-30), effective);
    }

    [Fact]
    public void Format_AnyDate_ReturnsRoundTripFormatLikeSaveTokens()
    {
        Assert.Equal("2026-10-28T13:32:00.8430000+00:00", AccessTokenExpiryHelper.Format(Expiry));
    }

    [Fact]
    public void TryParse_FormattedValue_ReturnsTrueAndSameDate()
    {
        Assert.True(AccessTokenExpiryHelper.TryParse(AccessTokenExpiryHelper.Format(Expiry), out var parsed));
        Assert.Equal(Expiry, parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-date")]
    public void TryParse_MissingOrInvalidValue_ReturnsFalse(string? raw)
    {
        Assert.False(AccessTokenExpiryHelper.TryParse(raw, out _));
    }
}
