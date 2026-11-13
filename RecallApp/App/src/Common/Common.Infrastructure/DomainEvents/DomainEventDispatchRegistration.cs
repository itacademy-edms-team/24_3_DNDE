using Common.Application.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Проводка доставки доменных событий: регистрирует диспетчер, который раздаёт события
/// обработчикам, зарегистрированным через <see cref="DomainEventRegistration"/>.
/// </summary>
public static class DomainEventDispatchRegistration
{
    public static IServiceCollection AddDomainEventDispatch(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();
        return services;
    }
}
