using Common.Application.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Предоставляет регистрацию доставки доменных событий.
/// </summary>
/// <remarks>
/// Регистрирует диспетчер, который раздаёт события обработчикам,
/// зарегистрированным через <see cref="DomainEventRegistration"/>.
/// </remarks>
public static class DomainEventDispatchRegistration
{
    /// <summary>
    /// Регистрирует диспетчер доменных событий.
    /// </summary>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <returns>Та же коллекция для цепочки регистраций.</returns>
    public static IServiceCollection AddDomainEventDispatch(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();
        return services;
    }
}
