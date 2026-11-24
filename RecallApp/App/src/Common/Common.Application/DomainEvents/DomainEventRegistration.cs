using System.Reflection;
using System.Runtime.CompilerServices;
using Common.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.DomainEvents;

/// <summary>
/// Предоставляет регистрацию обработчиков доменных событий.
/// </summary>
/// <remarks>
/// Сканирует сборку на реализации <see cref="IDomainEventHandler{TEvent}"/>, регистрирует их в DI
/// и заодно связывает стабильное имя события с его типом для десериализации воркером.
/// </remarks>
public static class DomainEventRegistration
{
    /// <summary>
    /// Регистрирует все реализации <see cref="IDomainEventHandler{TEvent}"/> из сборки.
    /// </summary>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <param name="assembly">Сборка модуля, содержащая обработчики.</param>
    /// <returns>Та же коллекция для цепочки регистраций.</returns>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        foreach (var handler in assembly.GetTypes().Where(type => type is { IsAbstract: false, IsInterface: false }))
        {
            var contracts = handler.GetInterfaces()
                .Where(contract => contract.IsGenericType
                                   && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var contract in contracts)
            {
                var eventType = contract.GetGenericArguments()[0];
                services.AddScoped(contract, handler);
                DomainEventTypeRegistry.Register(EventNameOf(eventType), eventType);
            }
        }

        return services;
    }

    /// <summary>
    /// Стабильное имя события читается с неинициализированного экземпляра:
    /// <see cref="IDomainEvent.EventType"/> - константа на тип и не зависит от состояния,
    /// поэтому конструктор вызывать не требуется.
    /// </summary>
    private static string EventNameOf(Type eventType)
    {
        var instance = (IDomainEvent)RuntimeHelpers.GetUninitializedObject(eventType);
        return instance.EventType;
    }
}
