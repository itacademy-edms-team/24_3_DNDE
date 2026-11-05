using Common.Application.Provisioning;
using Microsoft.Extensions.DependencyInjection;

namespace Api;

/// <summary>
/// Сквозная ступень JIT-провижининга (ADR 0020): для аутентифицированного запроса
/// гарантирует существование доменных строк, дергая все зарегистрированные провижинеры модулей.
/// </summary>
public static class DomainProvisioning
{
    public static IApplicationBuilder UseDomainProvisioning(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var provisioners = context.RequestServices.GetServices<IDomainProvisioner>();
            foreach (var provisioner in provisioners)
                await provisioner.EnsureProvisionedAsync(context.User, context.RequestAborted);
        }

        await next(context);
    });
}
