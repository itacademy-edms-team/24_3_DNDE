# Плагин http-логирования в БД

## Требования к решению

- .NET 8, ASP.NET Core.
- NLog как провайдер логирования.
- `Microsoft.EntityFrameworkCore` и `Microsoft.EntityFrameworkCore.Design` в проекте DbContext - для предоставления контекста и генерации миграции.

## Подключение

Подключите плагин к Host.

Добавьте в `Program.cs` два метода:

```csharp

builder.Services.AddHttpDatabaseLogging<YourLoggingDbContext>("app");

...

app = builder.Build();

// Global exception handler middlewares

app.UseHttpDatabaseLogging();

```

Добавьте в `DbContext`, отвечающем за лог:

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

Создайте и примените новую миграцию.

## Отключение

Уберите методы из пункта ["Подключение"](#подключение).

Уберите из `DbContext`, отвечающем за лог, ранее упомянутую строчку.

Создайте и примените новую миграцию.

## Возможные настройки и переопределения

Все настройки - опциональные аргументы `AddHttpDatabaseLogging<TDbContext>(...)`; обязателен только `serviceName`.

### service_name

```csharp
AddHttpDatabaseLogging<LoggingDbContext>("app");
```

### Маскирование - SanitizerOptions

```csharp
AddHttpDatabaseLogging<LoggingDbContext>("app",
    o => o.AddSensitiveKey("displayName").AddSensitiveKey("email"));
```

Добавляет ключи к стандартному набору; есть `RemoveSensitiveKey(...)`.

### Полная замена маскировщика - ISanitizer

Регистрируется в DI до плагина:

```csharp
builder.Services.AddSingleton<ISanitizer, MySanitizer>();
builder.Services.AddHttpDatabaseLogging<LoggingDbContext>("app");
```

### Захват тел - BodyCaptureOptions

```csharp
AddHttpDatabaseLogging<LoggingDbContext>("app",
    configureBodyCapture: o =>
    {
        o.CaptureRequestBody = false;
        o.MaxLength = 8192;
        o.ContentTypes.Add("application/custom");
    });
```

### Фильтр запросов - IRequestFilter

Регистрируется в DI до плагина:

```csharp
builder.Services.AddSingleton<IRequestFilter>(
    new ExcludePathPrefixRequestFilter("/health", "/static"));
builder.Services.AddHttpDatabaseLogging<LoggingDbContext>("app");
```

Для комбинации - `CompositeRequestFilter(...)`.

### Логгер, уровень, очередь - AccessLogOptions

```csharp
AddHttpDatabaseLogging<LoggingDbContext>("app",
    configureAccessLog: o =>
    {
        o.LoggerName = "MyApp.Access";
        o.MinLevel = NLog.LogLevel.Warn;
        o.QueueLimit = 20000;
    });
```


