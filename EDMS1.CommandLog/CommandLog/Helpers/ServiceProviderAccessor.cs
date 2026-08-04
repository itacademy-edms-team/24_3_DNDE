using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.Helpers;

/// <summary>
/// Хранит дочерний контейнер для доступа к Scoped сервисам в Singleton сервисах.
/// Нужно, например, для <see cref="RequestLogDecorator"/>.
/// https://github.com/dotnet/aspnetcore/blob/5116838807783a5beaf76a8a12e5faedb15a4f2d/src/Http/Http/src/HttpContextAccessor.cs
/// Реализация для храниния ServiceProviderHolder взята отсюда.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ServiceProviderAccessor"/> class.
/// </remarks>
public sealed class ServiceProviderAccessor(IHttpContextAccessor httpContextAccessor)
{
    private static readonly AsyncLocal<ServiceProviderHolder> ServiceProviderCurrent = new();

    /// <summary>
    /// Доступ к дочернему контейнеру, через который можно получить Scoped сервисы.
    /// </summary>
    public IServiceProvider? ServiceProvider =>
        httpContextAccessor.HttpContext is null
            ? ServiceProviderCurrent.Value?.ServiceProvider
            : httpContextAccessor.HttpContext.RequestServices;

    /// <summary>
    /// Задать дочерний контейнер. Вызывать, когда вызывается <see cref="ServiceProviderServiceExtensions.CreateScope"/>.
    /// </summary>
    /// <param name="value">Экземпляр <see cref="IServiceProvider"/>.</param>
    /// <param name="isNeedToClearPreviousInstance">Показывает необходимость в очистке предыдущего экземпляра.</param>
    public void SetProvider(IServiceProvider value, bool isNeedToClearPreviousInstance)
    {
        if (isNeedToClearPreviousInstance)
        {
            ClearCurrentProvider();
        }

        ServiceProviderCurrent.Value = new ServiceProviderHolder { ServiceProvider = value };
    }

    /// <summary>
    /// Очистить текущий дочерний контейнер. Вызывать перед <see cref="IServiceScope.Dispose"/>.
    /// </summary>
    public void ClearCurrentProvider()
    {
        var holder = ServiceProviderCurrent.Value;

        if (holder != null)
        {
            holder.ServiceProvider = null;
        }
    }

    private sealed class ServiceProviderHolder
    {
        public IServiceProvider? ServiceProvider { get; set; }
    }
}
