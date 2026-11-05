using Host.Configuration;

namespace Tests.Unit;

/// <summary>
/// Валидатор секции "Cors": конфигурации, которые браузер молча отвергнет
/// (wildcard-origin с cookie, origin с путём, пустые наборы), должны ронять старт.
/// </summary>
public sealed class CorsSectionOptionsValidatorTests
{
    private readonly CorsSectionOptionsValidator _validator = new();

    [Fact]
    public void Validate_EmptySection_Succeeds()
    {
        var result = _validator.Validate(null, new CorsSectionOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_PoliciesWithoutDefault_Fails()
    {
        var options = new CorsSectionOptions
        {
            Policies = { ["Browser"] = ValidBrowserPolicy() },
        };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("Default"));
    }

    [Fact]
    public void Validate_DefaultPolicyNotConfigured_Fails()
    {
        var options = new CorsSectionOptions
        {
            Default = "Browser",
            Policies = { ["Mobile"] = ValidBrowserPolicy() },
        };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("Mobile"));
    }

    [Fact]
    public void Validate_AnyOriginWithCredentials_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedOrigins = new CorsValueSet { Any = true };
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains(nameof(CorsPolicyOptions.AllowCredentials)));
    }

    [Fact]
    public void Validate_AnyOriginWithExplicitValues_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedOrigins.Any = true;
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("ambiguous"));
    }

    [Fact]
    public void Validate_OriginWithPath_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedOrigins.Values = ["https://app.localhost/account"];
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("path"));
    }

    [Fact]
    public void Validate_RelativeOrigin_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedOrigins.Values = ["app.localhost"];
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("absolute"));
    }

    [Fact]
    public void Validate_EmptyOriginsWithoutAny_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedOrigins.Values = [];
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("allows nothing"));
    }

    [Fact]
    public void Validate_UnknownHttpMethod_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedMethods = new CorsValueSet { Values = ["GET", "FETCH"] };
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("FETCH"));
    }

    [Fact]
    public void Validate_NegativePreflightMaxAge_Fails()
    {
        var policy = ValidBrowserPolicy();
        policy.PreflightMaxAge = TimeSpan.FromSeconds(-1);
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains(nameof(CorsPolicyOptions.PreflightMaxAge)));
    }

    [Fact]
    public void Validate_WildcardSubdomainOrigin_Succeeds()
    {
        var policy = ValidBrowserPolicy();
        policy.AllowedOrigins.Values = ["https://*.localhost"];
        var options = new CorsSectionOptions { Default = "Browser", Policies = { ["Browser"] = policy } };

        var result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ValidBrowserPolicy_Succeeds()
    {
        var options = new CorsSectionOptions
        {
            Default = "Browser",
            Policies = { ["Browser"] = ValidBrowserPolicy() },
        };

        var result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    private static CorsPolicyOptions ValidBrowserPolicy() =>
        new()
        {
            AllowCredentials = true,
            AllowedOrigins = new CorsValueSet { Values = ["https://app.localhost"] },
            AllowedHeaders = new CorsValueSet { Any = true },
            AllowedMethods = new CorsValueSet { Any = true },
            PreflightMaxAge = TimeSpan.FromHours(2),
        };
}
