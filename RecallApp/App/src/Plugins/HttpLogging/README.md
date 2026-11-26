# Http-логирование в БД

Плагин пишет access-логи HTTP-запросов в таблицу `http_logs` через NLog. Поддерживает два режима подключения:

- **Как модуль** - плагин владеет контекстом, миграциями и схемой, хост вызывает только методы подключения.
- **Как плагин** - хост передаёт в плагин свой DbContext и схему; миграции создаются и применяются на стороне хоста.

## Требования к решению

Общие:

- .NET 8, ASP.NET Core.
- NLog как провайдер логирования: `builder.Host.UseNLog()`.

Режим "модуль":

- Общий `NpgsqlDataSource` приложения (пул соединений).
- Строка подключения с паролем: NLog открывает собственные соединения и не может взять её из `NpgsqlDataSource`.
- Схема `logging` в БД и роли: владелец схемы (DDL, миграции) и runtime-роль (DML).
- Свободная квота в `ConnectionPoolBudget` под пул соединений NLog.

Режим "плагин":

- Свой DbContext лога с вызовом `UseDatabaseLogging(schema)` в `OnModelCreating`.
- `Microsoft.EntityFrameworkCore.Design` в проекте DbContext - для генерации миграций.

## Подключение

### Как модуль

Зарезервируйте квоту под пул NLog и зарегистрируйте плагин в `Program.cs`:

```csharp

var loggingPoolLease = poolBudget.GetPools(
    builder.Configuration.GetValue("AppDb:DatabaseLoggingPoolQuota", 5));
builder.Services.AddSingleton(new PoolLeaseHolder(loggingPoolLease));

builder.Services.AddHttpLogging(
    dataSource,
    connectionString, // строка подключения с паролем
    "app",
    configureSanitizer: o => o.AddSensitiveKey("displayName"));

```

Подключите middleware после глобальных обработчиков исключений:

```csharp

app.UseExceptionHandler();

app.UseHttpDatabaseLogging();

```

Примените миграции плагина (в режиме `--migrate` или при старте, под владельцем схемы):

```csharp

var loggingMigrationsConnectionString = builder.Configuration.GetConnectionString("LoggingMigrations")
    ?? throw new InvalidOperationException("Set ConnectionStrings__LoggingMigrations (logging schema owner).");
await HttpLoggingModule.MigrateHttpLogging(loggingMigrationsConnectionString);

```

Опционально добавьте проверку связности в `/health/ready`:

```csharp

app.Services.CanConnectHttpLogging();

```

Если передаёте нестандартную схему, передайте её и в `AddHttpLogging`, и в `MigrateHttpLogging`: иначе миграции уйдут в другую схему. Смена схемы требует перегенерации миграций.

### Как плагин

Подключите плагин к Host.

Добавьте в `Program.cs` два метода:

```csharp

builder.Services.AddHttpDatabaseLogging<YourLoggingDbContext>("app");

...

app = builder.Build();

// Global exception handler middlewares

app.UseHttpDatabaseLogging();

```

Добавьте в `DbContext`, отвечающем за лог, вызов `UseDatabaseLogging` с нужной схемой:

```csharp

public class YourLoggingDbContext(DbContextOptions<YourLoggingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Добавляем строчку ниже.
        modelBuilder.UseDatabaseLogging("logging");
        base.OnModelCreating(modelBuilder);
    }
}

```

Создайте и примените новую миграцию - в этом режиме миграции хранятся и применяются на стороне хоста.

## Отключение

### Как модуль

Уберите из `Program.cs` вызовы `AddHttpLogging`, `UseHttpDatabaseLogging` и `MigrateHttpLogging`, а также резервирование квоты под пул NLog.

Таблица `http_logs` остаётся в БД: данные не удаляются. Удалите схему отдельно, если она больше не нужна.

### Как плагин

Уберите методы из пункта ["Подключение"](#как-плагин).

Уберите из `DbContext`, отвечающем за лог, ранее упомянутую строчку.

Создайте и примените новую миграцию.

## Возможные настройки и переопределения

Все настройки - именованные аргументы `AddHttpLogging(...)`; обязателен только `serviceName`. В режиме плагина те же аргументы принимает `AddHttpDatabaseLogging<TDbContext>(...)`.

### service_name

```csharp
AddHttpLogging(dataSource, connectionString, "app");
```

### Схема и квота пула

```csharp
AddHttpLogging(dataSource, connectionString, "app",
    schema: "logging",
    poolQuota: 5);
```

`schema` фиксируется в сгенерированных миграциях: для её смены требуется пересоздать миграции, а значение указывать и при подключении, и при применении миграций.

`poolQuota` - размер пула соединений NLog. Он не входит в общий `ConnectionPoolBudget`, поэтому квота резервируется хостом отдельно.

### Маскирование - SanitizerOptions

```csharp
AddHttpLogging(dataSource, connectionString, "app",
    configureSanitizer: o => o.AddSensitiveKey("displayName").AddSensitiveKey("email"));
```

Добавляет ключи к стандартному набору; есть `RemoveSensitiveKey(...)`.

### Полная замена маскировщика - ISanitizer

Регистрируется в DI до подключения плагина:

```csharp
builder.Services.AddSingleton<ISanitizer, MySanitizer>();
builder.Services.AddHttpLogging(dataSource, connectionString, "app");
```

### Захват тел - BodyCaptureOptions

```csharp
AddHttpLogging(dataSource, connectionString, "app",
    configureBodyCapture: o =>
    {
        o.CaptureRequestBody = false;
        o.MaxLength = 8192;
        o.ContentTypes.Add("application/custom");
    });
```

### Фильтр запросов - IRequestFilter

Регистрируется в DI до подключения плагина:

```csharp
builder.Services.AddSingleton<IRequestFilter>(
    new ExcludePathPrefixRequestFilter("/health", "/static"));
builder.Services.AddHttpLogging(dataSource, connectionString, "app");
```

Для комбинации - `CompositeRequestFilter(...)`.

### Логгер, уровень, очередь - AccessLogOptions

```csharp
AddHttpLogging(dataSource, connectionString, "app",
    configureAccessLog: o =>
    {
        o.LoggerName = "MyApp.Access";
        o.MinLevel = NLog.LogLevel.Warn;
        o.QueueLimit = 20000;
    });
```
