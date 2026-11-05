using Host.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Host.Extensions;

/// <summary>
/// Класс расширений для добавления, использования и настройки Cors в проекте.
/// Для настройки используется секция "Cors".
/// </summary>
public static class CorsExtensions
{
    /// <summary>Потолок кэширования preflight в Chromium.</summary>
    private static readonly TimeSpan ChromiumPreflightCacheCap = TimeSpan.FromHours(2);

    /// <summary>
    /// Регистрирует политики из секции "Cors".
    /// </summary>
    /// <param name="services">Переданный <see cref="IServiceCollection"/></param>
    /// <param name="configuration">Переданный <see cref="IConfiguration"/></param>
    /// <returns>Переданный <paramref name="services"/> c настроенным Cors</returns>
    /// <exception cref="OptionsValidationException">Настройки в секции неправильно заданы.</exception>
    public static IServiceCollection AddCorsUsingOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var options =
            configuration.GetSection(CorsSectionOptions.SectionName).Get<CorsSectionOptions>()
            ?? new CorsSectionOptions();

        var validation = new CorsSectionOptionsValidator().Validate(null, options);
        if (validation.Failed)
        {
            throw new OptionsValidationException(
                CorsSectionOptions.SectionName,
                typeof(CorsSectionOptions),
                validation.Failures
            );
        }

        services.AddSingleton(Options.Create(options));

        services.AddCors(cors =>
        {
            foreach (var (policyName, policy) in options.Policies)
                cors.AddPolicy(policyName, builder => Apply(builder, policy));

            // Глобальный UseCors() без переданного аргумента применяет политику по умолчанию.
            if (!string.IsNullOrWhiteSpace(options.Default))
                cors.AddDefaultPolicy(builder => Apply(builder, options.Policies[options.Default]));
        });

        return services;
    }

    /// <summary>
    /// Подключает CORS глобально политикой по умолчанию из конфигурации.
    /// Вызывать после UseRouting и до UseAuthentication.
    /// </summary>
    public static IApplicationBuilder UseCorsUsingOptions(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<CorsSectionOptions>>().Value;
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Host.Cors");

        if (string.IsNullOrWhiteSpace(options.Default))
        {
            logger.LogWarning(
                "CORS: policy not set ({Section}:{Default} is empty). Browser-based clients will be blocked",
                CorsSectionOptions.SectionName,
                nameof(CorsSectionOptions.Default)
            );

            return app;
        }

        foreach (var (policyName, policy) in options.Policies)
            logger.LogInformation("CORS policy '{Policy}': {Description}", policyName, Describe(policy));
        logger.LogInformation(
            "CORS: the '{Policy}' policy is applied globally by default",
            options.Default
        );

        return app.UseCors();
    }

    private static void Apply(CorsPolicyBuilder builder, CorsPolicyOptions policy)
    {
        if (policy.AllowedOrigins.Any)
            builder.AllowAnyOrigin();
        else
            builder.WithOrigins(policy.AllowedOrigins.Values);

        // Без этого вызова "https://*.example.local" сравнивается буквально и не совпадает никогда.
        if (policy.AllowedOrigins.Values.Any(origin => origin.Contains('*')))
            builder.SetIsOriginAllowedToAllowWildcardSubdomains();

        if (policy.AllowedHeaders.Any)
            builder.AllowAnyHeader();
        else
            builder.WithHeaders(policy.AllowedHeaders.Values);

        if (policy.AllowedMethods.Any)
            builder.AllowAnyMethod();
        else
            builder.WithMethods(policy.AllowedMethods.Values);

        if (policy.ExposedHeaders.Length > 0)
            builder.WithExposedHeaders(policy.ExposedHeaders);

        if (policy.AllowCredentials)
            builder.AllowCredentials();

        if (policy.PreflightMaxAge is { } maxAge)
            builder.SetPreflightMaxAge(maxAge);
    }

    private static string Describe(CorsPolicyOptions policy)
    {
        var origins = policy.AllowedOrigins.Any
            ? "any"
            : string.Join(", ", policy.AllowedOrigins.Values);

        var credentials = policy.AllowCredentials ? "credentials" : "no-credentials";

        var preflight =
            policy.PreflightMaxAge is { } maxAge
                ? maxAge > ChromiumPreflightCacheCap
                    ? $"preflightMaxAge={maxAge} (capped to {ChromiumPreflightCacheCap} by Chromium)"
                    : $"preflightMaxAge={maxAge}"
                : "preflightMaxAge=default";

        return $"origins=[{origins}], {credentials}, {preflight}";
    }
}
