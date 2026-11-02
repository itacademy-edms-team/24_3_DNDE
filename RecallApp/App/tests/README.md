# Тесты App (модульный монолит)

Два проекта: `Tests.Architecture` и `Tests.Integration`. Категории trait пока нет — все тесты самодостаточны и не требуют запуска в контейнере.

| Проект | Что проверяет | Что нужно для запуска |
|---|---|---|
| `Tests.Architecture` | границы модулей (ADR 0029): разрешённые project references из csproj, «публична только композиция модуля», все сущности и FK в схеме своего модуля | ничего |
| `Tests.Integration` | конвейер хоста в процессе теста (`WebApplicationFactory<Program>`): анонимный `GET /api/users/me` → 401; бюджет пула соединений (`ConnectionPoolBudget`) | ничего |

Конфигурация тестового хоста (строки подключения, `Oidc:Authority`) — в `AppFactory.cs`, подключение к БД тесты не используют. Подключение к БД понадобится тестам JIT-провижининга: `ConnectionStrings__Database`/`AccountsMigrations` уже прокидываются в контейнере.

## Запуск в контейнере

Образ `tests/Dockerfile.Testing`, сервис `app-tests` в профиле `test` корневого compose. Сейчас тесты БД не трогают, но сервис подключён к `app-db` — для будущих тестов.

Из корня `RecallApp`, все тесты:

```powershell
docker compose --profile test run --rm --build app-tests
```

Фильтр по имени/классу — через `APP_TEST_FILTER`:

```powershell
$env:APP_TEST_FILTER='FullyQualifiedName~ConnectionPoolBudget'
docker compose --profile test run --rm app-tests
```

Отчёты `trx` складываются в `App/tests/TestResults/`.

## Запуск из IDE или `dotnet test`

Из папки `App/`:

```powershell
dotnet test
```

Тесты пока не требуют БД, Keycloak или переменных окружения - запускаются как обычно из Rider/VS или CLI.
