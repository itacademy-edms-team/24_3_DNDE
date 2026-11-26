using HttpLogging.Abstractions;
using HttpLogging.Sanitizers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NLog;

namespace HttpLogging;

/// <summary>
/// Набор расширений для подключений плагина к хосту.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Регистрирует плагин HTTP-логирования: HttpContextAccessor, middleware и NLog DatabaseTarget.
    /// </summary>
    /// <remarks>
    /// Чтобы полностью заменить стандартный маскировщик, зарегистрируйте собственную реализацию
    /// <see cref="ISanitizer"/> в DI до вызова этого метода. В таком случае
    /// <paramref name="configureSanitizer"/> игнорируется. Аналогично можно заменить
    /// <see cref="IRequestFilter"/> - зарегистрируйте свою реализацию до вызова.
    /// </remarks>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <param name="serviceName">Имя сервиса для колонки service_name.</param>
    /// <param name="configureSanitizer">Опциональная настройка маскирования сенситивных полей.</param>
    /// <param name="configureBodyCapture">Опциональная настройка захвата тел запроса/ответа.</param>
    /// <param name="configureAccessLog">Опциональная настройка логгера и минимального уровня записи.</param>
    /// <typeparam name="TDbContext">Хостовый DbContext, в модель которого добавлен UseDatabaseLogging().</typeparam>
    /// <returns><paramref name="services"/> для цепочки вызовов.</returns>
    public static IServiceCollection AddHttpDatabaseLogging<TDbContext>(
        this IServiceCollection services,
        string serviceName,
        Action<SanitizerOptions>? configureSanitizer = null,
        Action<BodyCaptureOptions>? configureBodyCapture = null,
        Action<AccessLogOptions>? configureAccessLog = null)
        where TDbContext : DbContext
    {
        services.AddHttpContextAccessor();
        services.AddSingleton(new HttpLoggingOptions { ServiceName = serviceName });
        services.AddHostedService(sp => ActivatorUtilities.CreateInstance<NLogDatabaseConfigurator<TDbContext>>(sp));

        var sanitizerOptions = new SanitizerOptions();
        configureSanitizer?.Invoke(sanitizerOptions);
        services.TryAddSingleton<ISanitizer>(_ => new DefaultSanitizer(sanitizerOptions));
        services.AddSingleton<HttpLoggingPipeline>();

        var bodyCaptureOptions = new BodyCaptureOptions();
        configureBodyCapture?.Invoke(bodyCaptureOptions);
        services.AddSingleton(bodyCaptureOptions);

        var accessLogOptions = new AccessLogOptions();
        configureAccessLog?.Invoke(accessLogOptions);
        services.AddSingleton(accessLogOptions);

        services.TryAddSingleton<IRequestFilter>(new LogAllRequestFilter());

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
        var pipeline = app.ApplicationServices.GetRequiredService<HttpLoggingPipeline>();
        LogManager.LogFactory.ServiceRepository.RegisterService(typeof(HttpLoggingPipeline), pipeline);

        return app.UseMiddleware<HttpPayloadCaptureMiddleware>();
    }
}
