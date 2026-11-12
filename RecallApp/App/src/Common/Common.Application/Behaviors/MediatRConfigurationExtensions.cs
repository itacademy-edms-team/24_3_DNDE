using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Behaviors;

/// <summary>Регистрация общих поведений конвейера MediatR для модуля.</summary>
public static class MediatRConfigurationExtensions
{
    /// <summary>
    /// Подключает валидацию команд и запросов (ADR 0038).
    /// Валидаторы модуля регистрируются отдельно через <c>AddValidatorsFromAssembly</c>.
    /// </summary>
    public static MediatRServiceConfiguration AddCommandValidation(this MediatRServiceConfiguration configuration)
    {
        configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        return configuration;
    }

    /// <summary>
    /// Регистрирует валидаторы FluentValidation из сборки Application-слоя модуля.
    /// </summary>
    public static IServiceCollection AddCommandValidators(this IServiceCollection services, Type applicationAssemblyMarker) =>
        services.AddValidatorsFromAssembly(applicationAssemblyMarker.Assembly);
}
