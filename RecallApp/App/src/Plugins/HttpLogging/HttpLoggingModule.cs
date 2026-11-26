using HttpLogging.Infrastructure.Data;
using HttpLogging.Sanitizers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HttpLogging;

/// <summary>
/// Предоставляет композицию плагина http-логирования:
/// регистрацию сервисов, миграции и проверку связности.
/// </summary>
/// <remarks>
/// Единственная публичная точка подключения плагина:
/// контекст, миграции и схема логов принадлежат плагину, хосту настраивать их не требуется.
/// </remarks>
public static class HttpLoggingModule
{
    /// <summary>
    /// Регистрирует плагин на общем пуле соединений приложения.
    /// </summary>
    /// <remarks>
    /// Квота соединений для пула NLog резервируется хостом через <c>ConnectionPoolBudget</c>
    /// до вызова метода и должна совпадать с <paramref name="poolQuota"/>.
    /// </remarks>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <param name="dataSource">Общий пул соединений приложения.</param>
    /// <param name="connectionString">
    /// Строка подключения с паролем для записи логов через NLog.
    /// <see cref="NpgsqlDataSource.ConnectionString"/> пароль не отдаёт, поэтому он передаётся явно.
    /// </param>
    /// <param name="serviceName">Имя сервиса для колонки service_name.</param>
    /// <param name="schema">Схема БД для таблицы логов.</param>
    /// <param name="poolQuota">Размер пула соединений NLog, пишущего логи.</param>
    /// <param name="configureSanitizer">Опциональная настройка маскирования сенситивных полей.</param>
    /// <param name="configureBodyCapture">Опциональная настройка захвата тел запроса/ответа.</param>
    /// <param name="configureAccessLog">Опциональная настройка логгера, уровня и очереди записи.</param>
    /// <returns>Та же коллекция для цепочки регистраций.</returns>
    public static IServiceCollection AddHttpLogging(
        this IServiceCollection services,
        NpgsqlDataSource dataSource,
        string connectionString,
        string serviceName,
        string schema = "logging",
        int poolQuota = 5,
        Action<SanitizerOptions>? configureSanitizer = null,
        Action<BodyCaptureOptions>? configureBodyCapture = null,
        Action<AccessLogOptions>? configureAccessLog = null)
    {
        services.AddSingleton(new HttpLoggingOptions
        {
            ServiceName = serviceName,
            Schema = schema,
            PoolQuota = poolQuota,
            ConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = poolQuota,
            }.ConnectionString,
        });
        services.AddDbContext<LoggingDbContext>(options => options.UseNpgsql(dataSource,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", schema)));

        return services.AddHttpDatabaseLogging<LoggingDbContext>(
            serviceName,
            configureSanitizer,
            configureBodyCapture,
            configureAccessLog);
    }

    /// <summary>
    /// Применяет миграции плагина.
    /// </summary>
    /// <param name="connectionString">Строка подключения пользователя-владельца схемы логов.</param>
    /// <param name="schema">Схема БД для таблицы логов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task MigrateHttpLogging(
        string connectionString,
        string schema = "logging",
        CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseNpgsql(connectionString, pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", schema))
            .Options;

        await using var db = new LoggingDbContext(options, new HttpLoggingOptions { Schema = schema });
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Проверяет, доступна ли схема логов.
    /// </summary>
    /// <param name="services">Провайдер сервисов хоста.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// <see langword="true"/>, если соединение с БД устанавливается;
    /// иначе <see langword="false"/>.
    /// </returns>
    public static Task<bool> CanConnectHttpLogging(
        this IServiceProvider services,
        CancellationToken cancellationToken = default) =>
        services.GetRequiredService<LoggingDbContext>().Database.CanConnectAsync(cancellationToken);
}
