using Host.Configuration;

namespace Tests.Unit;

/// <summary>
/// Валидатор секции "Spa": адрес SPA склеивается со строкой с путём возврата,
/// поэтому любой «хвост» (путь, query, слэш) должен ронять старт, а не давать кривой редирект.
/// </summary>
public sealed class SpaOptionsValidatorTests
{
    private readonly SpaOptionsValidator _validator = new();

    [Theory]
    [InlineData("https://app.localhost")]
    [InlineData("http://localhost:5173")]
    public void Validate_BareOrigin_Succeeds(string baseUrl)
    {
        var result = _validator.Validate(null, new SpaOptions { BaseUrl = baseUrl });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("app.localhost")]
    [InlineData("/")]
    [InlineData("https://app.localhost/")]
    [InlineData("https://app.localhost/account")]
    [InlineData("https://app.localhost?debug=1")]
    [InlineData("ftp://app.localhost")]
    public void Validate_NotABareOrigin_Fails(string baseUrl)
    {
        var result = _validator.Validate(null, new SpaOptions { BaseUrl = baseUrl });

        Assert.True(result.Failed);
    }
}
