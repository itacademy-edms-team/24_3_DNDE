using Microsoft.Extensions.Options;

namespace Host.Configuration;

/// <summary>
/// Валидатор секции "Cors". Падает на старте на конфигурациях, которые браузер молча отвергнет
/// и которые сложно заметить: wildcard-origin с cookie, пустые наборы значений,
/// ссылка Default на несуществующую политику.
/// </summary>
public sealed class CorsSectionOptionsValidator : IValidateOptions<CorsSectionOptions>
{
    private static readonly string[] KnownMethods =
    [
        "GET",
        "HEAD",
        "POST",
        "PUT",
        "PATCH",
        "DELETE",
        "OPTIONS",
    ];

    public ValidateOptionsResult Validate(string? name, CorsSectionOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Default))
        {
            if (options.Policies.Count > 0)
            {
                failures.Add(
                    $"'{CorsSectionOptions.SectionName}:{nameof(CorsSectionOptions.Default)}' is not set, "
                        + $"but {options.Policies.Count} policy(ies) are configured: the browser policy would never be applied."
                );
            }

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }

        if (!options.Policies.ContainsKey(options.Default))
        {
            failures.Add(
                $"'{CorsSectionOptions.SectionName}:{nameof(CorsSectionOptions.Default)}' = '{options.Default}' "
                    + $"does not match any configured policy (available: {FormatNames(options)})."
            );

            return ValidateOptionsResult.Fail(failures);
        }

        foreach (var (policyName, policy) in options.Policies)
            ValidatePolicy(policyName, policy, failures);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidatePolicy(string policyName, CorsPolicyOptions policy, List<string> failures)
    {
        var prefix = $"'{CorsSectionOptions.SectionName}:{nameof(CorsSectionOptions.Policies)}:{policyName}'";

        ValidateOrigins(prefix, policy, failures);
        ValidateValueSet(prefix, nameof(policy.AllowedHeaders), policy.AllowedHeaders, failures, isMethodSet: false);
        ValidateValueSet(prefix, nameof(policy.AllowedMethods), policy.AllowedMethods, failures, isMethodSet: true);

        if (policy.PreflightMaxAge is { } maxAge && maxAge < TimeSpan.Zero)
            failures.Add($"{prefix}:{nameof(policy.PreflightMaxAge)} must not be negative ('{maxAge}').");

        foreach (var header in policy.ExposedHeaders)
        {
            if (string.IsNullOrWhiteSpace(header))
                failures.Add($"{prefix}:{nameof(policy.ExposedHeaders)} contains an empty value.");
        }
    }

    private static void ValidateOrigins(string prefix, CorsPolicyOptions policy, List<string> failures)
    {
        var origins = policy.AllowedOrigins;

        if (origins.Any)
        {
            if (origins.Values.Length > 0)
            {
                failures.Add(
                    $"{prefix}:{nameof(policy.AllowedOrigins)} has Any = true and a non-empty Values list: "
                        + "remove one of them, the intent is ambiguous."
                );
            }

            // Тот самый случай, который браузер молча превращает в отказ: '*' + credentials несовместимы.
            if (policy.AllowCredentials)
            {
                failures.Add(
                    $"{prefix}:{nameof(policy.AllowedOrigins)}.Any = true cannot be combined with "
                        + $"{nameof(policy.AllowCredentials)} = true: the CORS protocol forbids a wildcard origin "
                        + "when cookies are sent. List explicit origins instead."
                );
            }

            return;
        }

        if (origins.Values.Length == 0)
        {
            failures.Add(
                $"{prefix}:{nameof(policy.AllowedOrigins)} allows nothing: set Any = true or list explicit origins."
            );

            return;
        }

        foreach (var origin in origins.Values)
            ValidateOrigin(prefix, origin, failures);
    }

    private static void ValidateOrigin(string prefix, string origin, List<string> failures)
    {
        var where = $"{prefix}:{nameof(CorsPolicyOptions.AllowedOrigins)}";

        if (string.IsNullOrWhiteSpace(origin))
        {
            failures.Add($"{where} contains an empty origin.");
            return;
        }

        var schemeSeparator = origin.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparator < 0)
        {
            failures.Add($"{where}: '{origin}' is not an absolute origin (expected scheme://host[:port]).");
            return;
        }

        var scheme = origin[..schemeSeparator];
        if (!string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{where}: '{origin}' uses unsupported scheme '{scheme}' (only http/https).");
            return;
        }

        var authority = origin[(schemeSeparator + 3)..];
        if (authority.Length == 0)
        {
            failures.Add($"{where}: '{origin}' has no host.");
            return;
        }

        // origin - это схема + хост (+ порт): путь, query и fragment в нём недопустимы,
        // иначе браузер никогда не совпадёт по строке.
        if (authority.IndexOfAny(['/', '?', '#']) >= 0)
            failures.Add($"{where}: '{origin}' must not contain a path, query or fragment.");

        var host = authority.Split(':')[0];
        if (host.Contains('*') && !host.StartsWith("*.", StringComparison.Ordinal))
            failures.Add($"{where}: '{origin}' has a misplaced wildcard (only 'https://*.example.local' form).");
    }

    private static void ValidateValueSet(
        string prefix,
        string propertyName,
        CorsValueSet set,
        List<string> failures,
        bool isMethodSet
    )
    {
        if (set.Any)
        {
            if (set.Values.Length > 0)
            {
                failures.Add(
                    $"{prefix}:{propertyName} has Any = true and a non-empty Values list: "
                        + "remove one of them, the intent is ambiguous."
                );
            }

            return;
        }

        if (set.Values.Length == 0)
        {
            failures.Add($"{prefix}:{propertyName} allows nothing: set Any = true or list explicit values.");
            return;
        }

        foreach (var value in set.Values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                failures.Add($"{prefix}:{propertyName} contains an empty value.");
                continue;
            }

            if (isMethodSet && !KnownMethods.Contains(value, StringComparer.OrdinalIgnoreCase))
                failures.Add($"{prefix}:{propertyName} contains unknown HTTP method '{value}'.");
        }
    }

    private static string FormatNames(CorsSectionOptions options) =>
        options.Policies.Count == 0 ? "none" : string.Join(", ", options.Policies.Keys);
}
