## Nuget

### Преднастройка

Откройте файл проекта и внесите изменения в следующую группу свойств (по необходимости):

```xml
    <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>EMDS1.CommandLog</PackageId>
    <Version>0.1.0</Version>
    <Authors>...</Authors>
    <Description>...</Description>
    <PackageTags>...</PackageTags>
    <PackageReadmeFile>...</PackageReadmeFile>
</PropertyGroup>
```

Создавать .nuspc файл не нужно, если свойства .csproj закрывают ваши потребности по упаковке и публикации.

Заполучите API-ключ репозитория, в который планируете публиковать пакет. Добавьте его в глобальный `Nuget.Config` в
раздел `<apikeys>` по примеру ниже:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
        ...
    </packageSources>
    ...
    <apikeys>
        <add key="https://api.nuget.org/v3/index.json" value="ваш_ключ" />
        ...
    </apikeys>
</configuration>
```

### Упаковка

! Перед упаковкой и публикацией обновите номер версии в свойствах проекта !

В корне решения выполните команду:

```
dotnet pack ./CommandLog/CommandLog.csproj -c Release -o ./nupkg
```

- В корне, в папке `nupkg/`, появится файл `EMDS1.CommandLog.<Version>.nupkg`, где:
    - `<Version>` - это версия пакета в свойствах проекта.

### Публикация

! Перед упаковкой и публикацией обновите номер версии в свойствах проекта !

В корне решения выполните команду:

```
dotnet nuget push ./nupkg/EMDS1.CommandLog.<Version>.nupkg \
    -s <Source> \
    --skip-duplicate
```

- где:
    - `<Version>` - это версия пакета в свойствах проекта.
    - `<Source>` - ссылка на `index.json` удалённого репозитория пакетов (Пример: https://api.nuget.org/v3/index.json).
- API-ключ подтянется из глобального `Nuget.Config`.
- `--skip-duplicate` позволяет не выдавать ошибку конфликта версий при попытке опубликовать уже хранящуюся в репозитории
  версию, а тихо завершить выполнение команды.

### Тестирование

Интеграционные тесты лежат в проекте `CommandLog.IntegrationTests`.

Запуск всех тестов из корня решения:

```powershell
dotnet test
```

Запуск тестов одного проекта:

```powershell
dotnet test ./CommandLog.IntegrationTests/CommandLog.IntegrationTests.csproj
```