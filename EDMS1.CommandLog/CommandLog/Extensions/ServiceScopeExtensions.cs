using EDMS1.CommandLog.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Helpers_ServiceProviderAccessor = EDMS1.CommandLog.Helpers.ServiceProviderAccessor;
using ServiceProviderAccessor = EDMS1.CommandLog.Helpers.ServiceProviderAccessor;

namespace EDMS1.CommandLog.Extensions;

/// <summary>
/// Расширения для <see cref="IServiceScope"/>.
/// </summary>
public static class ServiceScopeExtensions
{
    /// <summary>
    /// Задать провайдер в <see cref="scope"/>.
    /// </summary>
    /// <param name="scope">Экземпляр <see cref="isNeedToClearPreviousInstance"/>.</param>
    /// <param name="isNeedToClearPreviousInstance">Показывает необходимость в очистке предыдущего экземпляра.</param>
    public static void SetScopedProviderInAccessor(this IServiceScope scope, bool isNeedToClearPreviousInstance)
    {
        var accessor = scope.ServiceProvider.GetRequiredService<Helpers_ServiceProviderAccessor>();

        accessor.SetProvider(scope.ServiceProvider, isNeedToClearPreviousInstance);
    }

    /// <summary>
    /// Сохранить провайдер в <see cref="ServiceProviderAccessor"/>.
    /// </summary>
    public static void ClearScopedProviderInAccessor(this IServiceScope scope)
    {
        var accessor = scope.ServiceProvider.GetRequiredService<Helpers_ServiceProviderAccessor>();

        accessor.ClearCurrentProvider();
    }
}
