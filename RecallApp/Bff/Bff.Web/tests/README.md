# Тесты BFF

Два проекта: `Tests.Unit` и `Tests.Integration`. Интеграционные тесты делятся на две категории по trait `Category`.

| Проект, категория | Что проверяет | Что нужно для запуска |
|---|---|---|
| `Tests.Unit` | классы по отдельности: клиент token-endpoint, правило `expires_at`, окно cookie, ticket store, замки и уборщик, события входа OIDC | ничего |
| `Tests.Integration`, `InProcess` | весь конвейер BFF в процессе теста (`WebApplicationFactory`): refresh, параллельные запросы, backoff, отказ Keycloak, окно сессии. Keycloak и часы поддельные | ничего |
| `Tests.Integration`, `Keycloak` | refresh против настоящего Keycloak: форма ответа, ротация refresh-токенов, неверный секрет клиента | преднастроенный Keycloak с тестовым realm |

## Преднастроенный Keycloak

Категории `Keycloak` нужен realm `recallapp-test` с клиентом `Bff.Web.Test` (Direct Access Grants) и пользователем `testuser`. Настройка вручную: [Iam/README.md](../../../Iam/README.md), раздел «Тестовый realm». Секрет клиента и пароль пользователя держим в корневом `.env`:

```
BFF_TEST_OIDC_CLIENT_SECRET=...
BFF_TEST_USER_PASSWORD=...
```

Тесты получают токены входом по паролю, минуя страницы Keycloak, и кладут их в сессию BFF тестовым endpoint-ом. Часы BFF поддельные, поэтому refresh запускается сдвигом часов, без ожидания срока access-токена.

## Запуск в контейнере

Образ `Dockerfile.Testing`, сервис `bff-tests` в профиле `test` корневого compose. Нужен поднятый стек (`docker compose up`): тесты ходят в Keycloak через `edge-proxy` по `https://auth.localhost`.

Из корня `RecallApp`, все тесты:

```bash
docker compose --profile test run --rm --build bff-tests
```

Без категории `Keycloak` (стек и тестовый realm не нужны):

```bash
BFF_TEST_FILTER='Category!=Keycloak' docker compose --profile test run --rm --build bff-tests
```

```powershell
$env:BFF_TEST_FILTER='Category!=Keycloak'
docker compose --profile test run --rm --build bff-tests
```

Отчёты `trx` складываются в `tests/TestResults/`.

## Запуск из IDE или `dotnet test`

`Tests.Unit` и категория `InProcess` запускаются как обычно. Тесты категории `Keycloak` без переменных окружения `KeycloakTests__*` пропускаются (Skip) с подсказкой про контейнер.
