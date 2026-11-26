using HttpLogging.Sanitizers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HttpLogging;

/// <summary>
/// Набор расширений для подключений плагина к хосту.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Регистрирует плагин HTTP-логирования: HttpContextAccessor, middleware и NLog DatabaseTarget.
    /// </summary>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <param name="serviceName">Имя сервиса для колонки service_name.</param>
    /// <param name="configureSanitizer">Опциональная настройка маскирования сенситивных полей.</param>
    /// <typeparam name="TDbContext">Хостовый DbContext, в модель которого добавлен UseDatabaseLogging().</typeparam>
    /// <returns><paramref name="services"/> для цепочки вызовов.</returns>
    public static IServiceCollection AddHttpDatabaseLogging<TDbContext>(
        this IServiceCollection services,
        string serviceName,
        Action<SanitizerOptions>? configureSanitizer = null)
        where TDbContext : DbContext
    {
        services.AddHttpContextAccessor();
        services.AddSingleton(new HttpLoggingOptions { ServiceName = serviceName });
        services.AddHostedService(sp => ActivatorUtilities.CreateInstance<NLogDatabaseConfigurator<TDbContext>>(sp));

        if (configureSanitizer is not null)
        {
            var options = new SanitizerOptions();
            configureSanitizer(options);
            HttpLoggingPipeline.Sanitizer = new DefaultSanitizer(options);
        }

        RendererRegistration.Register();
        return services;
    }

    /// <summary>
    /// Подключает middleware для корректной работы плагина с HTTP-запросом/ответом.
    /// </summary>
    /// <remarks>
    /// Подключается в начале middleware-пайплайна, после глобальных обработчиков исключений.
    /// </remarks>
    /// <param name="app">Строитель ASP.NET приложения.</param>
    /// <returns><paramref name="app"/> для цепочки вызовов.</returns>
    public static IApplicationBuilder UseHttpDatabaseLogging(this IApplicationBuilder app)
    {
        HttpLoggingPipeline.HttpContextAccessor =
            app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();

        return app.UseMiddleware<HttpPayloadCaptureMiddleware>();
    }
}
