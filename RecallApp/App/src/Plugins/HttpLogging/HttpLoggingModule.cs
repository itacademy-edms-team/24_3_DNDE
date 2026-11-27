using HttpLogging.Infrastructure.Data;
using HttpLogging.Sanitizers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

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
    private const string MigrationsHistoryTable = "__EFMigrationsHistory";
    
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
        string? schema = null,
        int poolQuota = 5,
        Action<SanitizerOptions>? configureSanitizer = null,
        Action<BodyCaptureOptions>? configureBodyCapture = null,
        Action<AccessLogOptions>? configureAccessLog = null)
    {
        var logOptions = new HttpLoggingOptions
        {
            ServiceName = serviceName,
            PoolQuota = poolQuota,
            ConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = poolQuota,
            }.ConnectionString,
        };
        if (schema is not null)
            logOptions.Schema = schema;
        services.AddSingleton(logOptions);
        
        services.AddDbContext<LoggingDbContext>(dbOptions =>
            dbOptions.UseNpgsql(dataSource, pg => ConfigureMigrationsHistory(pg, logOptions.Schema)));

        return services.AddHttpDatabaseLogging<LoggingDbContext>(
            serviceName,
            configureSanitizer,
            configureBodyCapture,
            configureAccessLog);
    }

    /// <summary>
    /// Применяет миграции плагина.
    /// </summary>
    /// <remarks>
    /// <paramref name="schema"/> согласуется с указанной в методе <see cref="AddHttpLogging"/>
    /// </remarks>
    /// <param name="connectionString">Строка подключения пользователя-владельца схемы логов.</param>
    /// <param name="schema">Схема БД для таблицы логов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task MigrateHttpLogging(
        string connectionString,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var logOptions = new HttpLoggingOptions();
        if (schema is not null)
            logOptions.Schema = schema;

        var dbOptions = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseNpgsql(connectionString, pg => ConfigureMigrationsHistory(pg, logOptions.Schema))
            .Options;

        await using var db = new LoggingDbContext(dbOptions, logOptions);
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

    private static void ConfigureMigrationsHistory(NpgsqlDbContextOptionsBuilder pg, string schema) =>
        pg.MigrationsHistoryTable(MigrationsHistoryTable, schema);
}
