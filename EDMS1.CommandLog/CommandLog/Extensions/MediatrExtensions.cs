using EDMS1.CommandLog.Mediatr;
using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.Extensions;

/// <summary>
/// Набор расширений для конфигурации CommandLog в MediatR.
/// </summary>
public static class MediatrExtensions
{
    /// <summary>
    /// Регистрирует <see cref="CommandLogBehavior{TRequest,TResponse}"/> при конфигурации MediatR.
    /// Вызывать внутри <c>AddMediatR</c>.
    /// </summary>
    /// <param name="configuration">Конфигурация MediatR.</param>
    /// <returns>Та же конфигурация MediatR для построения цепочки вызовов.</returns>
    public static MediatRServiceConfiguration AddCommandLogBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(CommandLogBehavior<,>));
    }
}