# CommandLog

Модуль журнала команд. Предоставляет функциональность для записи команд MediatR в базу данных.

## Command result and status
По умолчанию, если команда выполнена - будет статус `Successful`  
Если завершилась с исключением - статус `Failed`  
Если нужно задать другой статус, например, что операция была завершена, но не по основному сценарию (статус `CompletedWithVariation`),
используйте для `IRequestHandler` в качестве возвращаемого значения `CommandResult` и с помощью него передайте статус и комментарий

## Setup

В приложении-хосте в DbContext, который будет использовать CommandLog:
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

Там, где регистрируете сервисы, вызовите `AddCommandLogService`, указав два параметра:
- **Обобщённый параметр `<TContext>`** — `DbContext`, где используется `UseCommandLog();`. Сервис журнала будет
писать команды именно в этот контекст.
- **Аргумент `assemblyMarkerType`** — любой тип из сборки, где лежат ваши команды (`ICommand`). Нужен чтобы найти эту сборку и просканировать её: модуль составит список всех команд и их имён. Список используется при повторной обработке (retry), чтобы восстановить команду из сохранённого JSON.
```csharp
using EDMS1.CommandLog.Extensions;

...
builder.Services.AddCommandLogService<AppDbContext>(typeof(SomeCommand));
...
```

Там, где настраивается MediatR, вызовите `AddCommandLogBehavior();`:
```csharp
builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssembly(typeof(SomeCommand).Assembly);
        ...
        cfg.AddCommandLogBehavior();
        ...
    });
```

Там, где настраивается OData, вызовите `AddCommandLogOData();`:
```csharp
var odata = new ODataConventionModelBuilder();
...
odata.AddCommandLogOData();
...
```

## Contributing

см. [CONTRIBUTING.md](CONTRIBUTING.md)