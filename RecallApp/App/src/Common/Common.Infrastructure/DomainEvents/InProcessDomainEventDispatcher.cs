using System.Collections;
using System.Collections.Concurrent;
using Common.Application.DomainEvents;
using Common.Domain.DomainEvents;
using ErrorOr;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.DomainEvents;

/// <summary>
/// Доставляет доменные события их обработчикам.
/// </summary>
/// <remarks>
/// <para>Обработчики резолвятся напрямую из DI (без MediatR)
/// и вызываются через делегат, кэшируемый по типу события.</para>
/// <para>Ошибки всех обработчиков собираются в один результат: доставка не останавливается
/// на первом отказе.</para>
/// </remarks>
public sealed class InProcessDomainEventDispatcher(IServiceProvider services) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, IDomainEvent, CancellationToken, Task<ErrorOr<Success>>>> Invokers = new();

    public async Task<ErrorOr<Success>> DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        foreach (var domainEvent in events)
        {
            var result = await InvokeAsync(domainEvent, cancellationToken);
            if (result.IsError)
                errors.AddRange(result.Errors);
        }

        return errors.Count == 0 ? Result.Success : errors.ToList();
    }

    private Task<ErrorOr<Success>> InvokeAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var invoker = Invokers.GetOrAdd(domainEvent.GetType(), BuildInvoker);
        return invoker(services, domainEvent, cancellationToken);
    }

    private static Func<IServiceProvider, IDomainEvent, CancellationToken, Task<ErrorOr<Success>>> BuildInvoker(Type eventType)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        return async (provider, domainEvent, cancellationToken) =>
        {
            var handlers = (IEnumerable)provider.GetRequiredService(handlersType);
            var errors = new List<Error>();

            foreach (var handler in handlers)
            {
                var task = (Task<ErrorOr<Success>>)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                var result = await task;
                if (result.IsError)
                    errors.AddRange(result.Errors);
            }

            return errors.Count == 0 ? Result.Success : errors.ToList();
        };
    }
}
