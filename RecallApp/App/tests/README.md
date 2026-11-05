# Тесты App (модульный монолит)

Два проекта: `Tests.Architecture` и `Tests.Integration`. Интеграционные тесты делятся по trait `Category`.

| Проект, категория | Что проверяет | Что нужно для запуска |
|---|---|---|
| `Tests.Architecture` | границы модулей (ADR 0029): разрешённые project references из csproj, «публична только композиция модуля», все сущности и FK в схеме своего модуля | ничего |
| `Tests.Integration` (без категории) | конвейер хоста в процессе теста (`WebApplicationFactory<Program>`): анонимный `GET /api/users/me` → 401; бюджет пула соединений (`ConnectionPoolBudget`) | ничего |
| `Tests.Integration`, `Category=Database` | JIT-провижининг (ADR 0020) против живой app_db: аутентифицированный `GET /api/users/me` создаёт доменную строку (upsert идемпотентен, id стабилен) | app_db (контейнер) |

Конфигурация тестового хоста — в `AppFactory.cs`: тестовая аутентификация (claims по заголовку `X-Test-Auth`), опциональная подмена claim `sub` заголовком `X-Test-Sub` (проверка 400 `InvalidKeycloakSub`), строки подключения берутся из env контейнера, локально подменяются заглушками. `DatabaseFact` пропускает тест, если не задан `ConnectionStrings__AccountsMigrations` (паттерн `KeycloakFact` из BFF). 

Общие правила тестирования: [docs/testing-conventions.md](../../docs/testing-conventions.md).

## Запуск в контейнере

Образ `tests/Dockerfile.Testing`, сервис `app-tests` в профиле `test` корневого compose. Тесты категории `Database` применяют миграции к app_db (`DatabaseFixture`, owner-подключение).

Из корня `RecallApp`, все тесты:

```powershell
docker compose --profile test run --rm --build app-tests
```

Без БД (миграции и Database-тесты не нужны):

```powershell
$env:APP_TEST_FILTER='Category!=Database'
docker compose --profile test run --rm app-tests
```

Только Database-тесты:

```powershell
$env:APP_TEST_FILTER='Category=Database'
docker compose --profile test run --rm app-tests
```

Отчёты `trx` складываются в `App/tests/TestResults/`.

## Запуск из IDE или `dotnet test`

Из папки `App/`:

```powershell
dotnet test
```

Тесты без категории `Database` не требуют БД, Keycloak или переменных окружения. Тесты `Database` без `ConnectionStrings__AccountsMigrations` пропускаются (Skip) с подсказкой про контейнер.
