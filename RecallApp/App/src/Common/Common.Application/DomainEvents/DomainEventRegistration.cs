using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.DomainEvents;

/// <summary>
/// Регистрация обработчиков доменных событий: сканирует сборку на реализации
/// <see cref="IDomainEventHandler{TEvent}"/> и регистрирует их по нашему маркеру.
/// </summary>
public static class DomainEventRegistration
{
    /// <summary>Регистрирует все реализации <see cref="IDomainEventHandler{TEvent}"/> из сборки как scoped.</summary>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        foreach (var handler in assembly.GetTypes().Where(type => type is { IsAbstract: false, IsInterface: false }))
        {
            var contracts = handler.GetInterfaces()
                .Where(contract => contract.IsGenericType
                                   && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var contract in contracts)
                services.AddScoped(contract, handler);
        }

        return services;
    }
}
