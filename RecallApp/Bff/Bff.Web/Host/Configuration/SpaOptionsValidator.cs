using Microsoft.Extensions.Options;

namespace Host.Configuration;

/// <summary>
/// Валидатор секции "Spa": адрес SPA склеивается со строкой с путём возврата,
/// поэтому он обязан быть чистым origin'ом - без пути, query, fragment и хвостового слэша.
/// </summary>
public sealed class SpaOptionsValidator : IValidateOptions<SpaOptions>
{
    public ValidateOptionsResult Validate(string? name, SpaOptions options)
    {
        var where = $"{SpaOptions.SectionName}:{nameof(SpaOptions.BaseUrl)}";
        var value = options.BaseUrl;

        if (string.IsNullOrWhiteSpace(value))
            return ValidateOptionsResult.Fail($"'{where}' is not set: redirects from BFF have nowhere to go.");

        var schemeSeparator = value.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparator < 0)
            return ValidateOptionsResult.Fail($"'{where}' = '{value}' is not an absolute URL (expected scheme://host[:port]).");

        var scheme = value[..schemeSeparator];
        if (!string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail($"'{where}' = '{value}' uses unsupported scheme '{scheme}' (only http/https).");
        }

        var authority = value[(schemeSeparator + 3)..];
        if (authority.IndexOfAny(['/', '?', '#']) >= 0)
            return ValidateOptionsResult.Fail($"'{where}' = '{value}' must be a bare origin (no path, query or fragment).");

        if (authority.Length == 0)
            return ValidateOptionsResult.Fail($"'{where}' = '{value}' has no host.");

        return ValidateOptionsResult.Success;
    }
}
