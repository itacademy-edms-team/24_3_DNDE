# CommandLog

Модуль журнала команд. Предоставляет функциональность для записи команд MediatR в базу данных.

## Command result and status

По умолчанию, если команда выполнена - будет статус `Successful`  
Если завершилась с исключением - статус `Failed`  
Если нужно задать другой статус, например, что операция была завершена, но не по основному сценарию (статус
`CompletedWithVariation`),
используйте для `IRequestHandler` в качестве возвращаемого значения `CommandResult` и с помощью него передайте статус и
комментарий

## Setup

В приложении-хосте в DbContext, который будет использовать CommandLog, вызовите `UseCommandLog();`:

```csharp
using EDMS1.CommandLog.Extensions;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Добавляет таблицу CommandLog в контекст хоста
        builder.UseCommandLog();
    }
}
```

Там, где регистрируете сервисы, вызовите `AddCommandLogService<TContext>`:

- **Обобщённый параметр `<TContext>`** — `DbContext`, где используется `UseCommandLog();`. Сервис журнала будет
  писать команды именно в этот контекст.

```csharp
using EDMS1.CommandLog.Extensions;

...
builder.Services.AddCommandLogService<AppDbContext>();
...
```

Там, где настраивается MediatR, зарегистрируйте все сборки, в которых используются команды и вызовите
`AddCommandLogBehavior();`:

```csharp
builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssembly(typeof(CommandFromAssembly1).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(CommandFromAssembly2).Assembly);
        ...
        cfg.AddCommandLogBehavior();
        ...
    });
```

- Убедитесь, что в разных сборках нет одинаковых имён команд. CommandTypeResolver, который сканирует сборки на наличие
  команд, выдаст ошибку дубликатов имён.

Там, где настраивается OData, вызовите `AddCommandLogOData();`:

```csharp
var odata = new ODataConventionModelBuilder();
...
odata.AddCommandLogOData();
...
```

## Contributing

см. [CONTRIBUTING.md](CONTRIBUTING.md)