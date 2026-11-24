using Common.Application.Provisioning;
using Microsoft.Extensions.DependencyInjection;

namespace Api;

/// <summary>
/// Предоставляет ступень JIT-провижининга (ADR 0020).
/// </summary>
/// <remarks>
/// Для аутентифицированного запроса вызывает все зарегистрированные провижинеры модулей
/// и публикует идентификатор доменного пользователя в <see cref="ICurrentUser"/> (ADR 0036).
/// </remarks>
public static class DomainProvisioning
{
    /// <summary>
    /// Подключает провижининг доменных строк в конвейер запросов.
    /// </summary>
    /// <param name="app">Построитель приложения.</param>
    /// <returns>Тот же построитель для цепочки.</returns>
    public static IApplicationBuilder UseDomainProvisioning(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var currentUser = context.RequestServices.GetRequiredService<CurrentUser>();
            var provisioners = context.RequestServices.GetServices<IDomainProvisioner>();
            foreach (var provisioner in provisioners)
            {
                var userId = await provisioner.EnsureProvisionedAsync(context.User, context.RequestAborted);
                if (userId is { } id)
                    currentUser.SetUserId(id);
            }
        }

        await next(context);
    });
}
