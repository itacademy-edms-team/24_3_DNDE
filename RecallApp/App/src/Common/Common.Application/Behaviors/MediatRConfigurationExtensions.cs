using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Behaviors;

/// <summary>
/// Предоставляет методы расширения для регистрации общих поведений MediatR.
/// </summary>
public static class MediatRConfigurationExtensions
{
    /// <summary>
    /// Подключает валидацию команд и запросов в конвейер MediatR.
    /// </summary>
    /// <remarks>
    /// Валидаторы модуля регистрируются отдельно через <see cref="AddCommandValidators"/>
    /// (ADR 0038).
    /// </remarks>
    /// <param name="configuration">Конфигурация MediatR модуля.</param>
    /// <returns>Та же конфигурация для цепочки.</returns>
    public static MediatRServiceConfiguration AddCommandValidation(this MediatRServiceConfiguration configuration)
    {
        configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        return configuration;
    }

    /// <summary>
    /// Регистрирует валидаторы FluentValidation из сборки Application-слоя модуля.
    /// </summary>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <param name="applicationAssemblyMarker">Тип из сборки Application-слоя модуля.</param>
    /// <returns>Та же коллекция для цепочки регистраций.</returns>
    public static IServiceCollection AddCommandValidators(this IServiceCollection services, Type applicationAssemblyMarker) =>
        services.AddValidatorsFromAssembly(applicationAssemblyMarker.Assembly);
}
