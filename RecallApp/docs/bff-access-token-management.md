# Duende.AccessTokenManagement вместо своего refresh в BFF: исследование для решения

**Статус:** решение отложено до начала C1 (2026-10-29). C0 доводим на своём решении.

**Почему отложено.** Объём своего кода после перехода почти не меняется: политика исходов, окно и защита от позднего запроса остаются своими. Польза ATM (обработчик исходящих вызовов, основа Token Exchange, revoke, DPoP) появляется только с первым вызовом API, а переход сейчас требует переписать и заново проверить защиту от позднего запроса (§5.1). Частичное внедрение (ATM только для исходящих вызовов) не берём: обработчик ATM сам делает refresh, и получились бы два пути refresh.

**Пересматриваем при любом из триггеров:** первый исходящий вызов API или Token Exchange (C1); нужен DPoP; нужны client credentials для межсервисных вызовов; переход на Redis и несколько экземпляров BFF.

**Вопрос:** заменить ли самописное обновление токенов C0 ([bff-session-design.md](bff-session-design.md)) библиотекой `Duende.AccessTokenManagement` (дальше ATM)? Если да, то как её встроить и что остаётся своим кодом.

**Основа:** документация Duende, NuGet и исходники ATM. Код смотрел в `DuendeSoftware/foss`, ветка `main`, коммит `c428b7b` от 2026-10-21; это на пять дней новее релиза 4.3.0. Утверждения о поведении взяты из кода. Где источник другой, это помечено.

## Содержание

- [Duende.AccessTokenManagement вместо своего refresh в BFF: исследование для решения](#duendeaccesstokenmanagement-вместо-своего-refresh-в-bff-исследование-для-решения)
  - [Содержание](#содержание)
  - [1. Коротко](#1-коротко)
  - [2. Что такое ATM](#2-что-такое-atm)
  - [3. Как ATM обновляет пользовательский токен](#3-как-atm-обновляет-пользовательский-токен)
  - [4. Наш код против ATM](#4-наш-код-против-atm)
  - [5. Чего ATM не делает и где он нам опасен](#5-чего-atm-не-делает-и-где-он-нам-опасен)
    - [5.1. Поздний запрос с отозванным refresh-токеном (критично)](#51-поздний-запрос-с-отозванным-refresh-токеном-критично)
    - [5.2. Нет sign-out при отказе и нет backoff](#52-нет-sign-out-при-отказе-и-нет-backoff)
    - [5.3. Таймаут token endpoint](#53-таймаут-token-endpoint)
    - [5.4. Часы](#54-часы)
    - [5.5. Окно cookie](#55-окно-cookie)
    - [5.6. Масштабирование](#56-масштабирование)
    - [5.7. Что ATM не касается вообще](#57-что-atm-не-касается-вообще)
  - [6. Таблица слабых мест C0 после ATM](#6-таблица-слабых-мест-c0-после-atm)
  - [7. Token Exchange поверх ATM](#7-token-exchange-поверх-atm)
    - [7.1. Что есть в ATM](#71-что-есть-в-atm)
    - [7.2. Схема](#72-схема)
    - [7.3. Требования Keycloak 26 (Standard Token Exchange, V2)](#73-требования-keycloak-26-standard-token-exchange-v2)
  - [8. Ветка «если добавим Duende.BFF»](#8-ветка-если-добавим-duendebff)
  - [9. Предлагаемая схема интеграции](#9-предлагаемая-схема-интеграции)
  - [10. Окно сессии (ADR 0027): что меняется](#10-окно-сессии-adr-0027-что-меняется)
  - [11. План миграции](#11-план-миграции)
  - [12. Решения, нужные от владельца](#12-решения-нужные-от-владельца)
  - [13. Источники](#13-источники)

## 1. Коротко

- ATM подходит по лицензии (Apache 2.0), поддерживает .NET 8 и стыкуется с нашим `SaveTokens` и `ITicketStore` без переделки входа.
- ATM берёт на себя запрос к token endpoint, запись токенов в сессию, single-flight внутри процесса, HTTP-обработчик для исходящих вызовов, отзыв refresh-токена при выходе, DPoP и метрики OpenTelemetry.
- ATM **не** закрывает большую часть нашей таблицы слабых мест. Он не делает sign-out при отказе Keycloak, не делает backoff, не держит окно cookie и не даёт 401-контракт, CSRF и распределённый замок.
- **Главный риск.** Наш Keycloak ротирует refresh-токены (Revoke Refresh Token `ON`, Max Reuse `0`). Замок ATM группирует только запросы, которые встретились, пока refresh идёт. Запрос, прочитавший сессию до refresh и пришедший к замку после, повторит refresh отозванным токеном и получит `invalid_grant`. Это родственник бага сценария 9 (§8.1 дизайна C0). Наш double-check по `LastRefreshedAt` этот случай закрывает, ATM нет. Значит, свой `IUserTokenRequestConcurrencyControl` обязателен с первого дня.
- Token Exchange в ATM нет. Его делаем сами поверх публичных точек расширения ATM (`AccessTokenRequestHandler.ITokenRetriever`) и `Duende.IdentityModel`.
- **Предварительная оценка.** Своего кода станет меньше примерно наполовину. Исчезнут транспорт и запись токенов, `AccessTokenExpiryHelper`, уборщик замков. Политика сессии (Rejected, backoff, окно) остаётся своей, но станет тоньше. Главный выигрыш даже не в объёме кода, а в готовом шве для исходящих вызовов и Token Exchange.

## 2. Что такое ATM

| Параметр | Значение |
|---|---|
| Пакеты | `Duende.AccessTokenManagement` (client credentials, общие типы, `AccessTokenRequestHandler`), `Duende.AccessTokenManagement.OpenIdConnect` (пользовательские токены) |
| Последняя версия | 4.3.0 от 2026-10-16. Линия 4.x выходит с 2025-09 |
| TFM пакета | `net8.0`, `net9.0`, `net10.0` (ветка `main` в репозитории собирается под `net10.0`, пакет 4.3.0 мультитаргетный) |
| Зависимости под `net8.0` | `Microsoft.AspNetCore.Authentication.OpenIdConnect >= 8.0.32` (у нас ровно 8.0.32), `Microsoft.IdentityModel.Protocols.OpenIdConnect >= 8.19.2 < 9`. Транзитивно `Duende.IdentityModel`, `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.Http.Resilience` |
| Лицензия | Apache 2.0 (в заголовке каждого файла и на NuGet). Коммерческая лицензия Duende не нужна |
| Репозиторий | `DuendeSoftware/foss`, папка `access-token-management`. Issues и PR открыты |

Лицензионная проблема относится к **Duende.BFF** и IdentityServer, а не к ATM ([§8](#8-ветка-если-добавим-duendebff)).

## 3. Как ATM обновляет пользовательский токен

Регистрация: `services.AddOpenIdConnectAccessTokenManagement(o => …)`. Главные сервисы:

| Тип | Время жизни | Роль |
|---|---|---|
| `IUserTokenManager` (`UserAccessAccessTokenManager`) | transient | Решает, нужен ли refresh, вызывает его под контролем конкуренции, сохраняет результат |
| `IUserTokenStore` (`AuthenticationSessionUserAccessTokenStore`) | scoped | Читает и пишет токены в `AuthenticationProperties` cookie-схемы, после записи делает `SignInAsync` |
| `IStoreTokensInAuthenticationProperties` | singleton | Формат токенов в properties: `.Token.access_token`, `.Token.refresh_token`, `.Token.expires_at`, `.Token.token_type`, `.Token.client_id` |
| `IUserTokenRequestConcurrencyControl` | singleton | Single-flight: `ConcurrentDictionary<ключ, Lazy<Task<…>>>` |
| `IOpenIdConnectUserTokenEndpoint` | transient | POST на token endpoint (refresh) и revocation endpoint |
| `IOpenIdConnectConfigurationService` | transient | Берёт `ClientId`, секрет, discovery и **`Backchannel` HttpClient из `OpenIdConnectOptions`** |
| `AuthenticateResultCache` | scoped | Кэш результата аутентификации на запрос, чтобы после своего SignIn не читать старый тикет |

Алгоритм `GetAccessTokenAsync(user, parameters)`:

1. Пользователь не аутентифицирован: `FailedResult("No active user")`.
2. Из properties текущего запроса читаются access, refresh и `expires_at`.
3. Refresh нужен, если `ForceTokenRenewal`, или токена нет, или `expires_at < now + RefreshBeforeExpiration` (по умолчанию 1 мин, `now` из `TimeProvider`).
4. Ключ single-flight = значение refresh-токена + хэш DPoP-ключа + resource, scope, схемы, assertion и доп. параметры (так с 4.3, issue #360).
5. Первый запрос с этим ключом создаёт `Lazy<Task>`: POST `grant_type=refresh_token`, при успехе `IUserTokenStore.StoreTokenAsync`, то есть запись в properties и `SignInAsync`. Остальные запросы с тем же ключом ждут ту же задачу и получают тот же результат.
6. В `finally` запись словаря удаляется. Память не копится, уборщик не нужен.
7. Ошибка Keycloak: `FailedResult(Error, ErrorDescription)`, warning в лог. **Сессию ATM не трогает.**

Исходящие вызовы: `AddUserAccessTokenHttpClient(...)` или `AddHttpClient<T>().AddUserAccessTokenHandler()`. `AccessTokenRequestHandler` (это `DelegatingHandler`) берёт токен через `ITokenRetriever` и ставит `Authorization: Bearer`, либо `DPoP` вместе с proof. Если токен получить не удалось, **запрос уходит без токена**.

Выход: `HttpContext.RevokeRefreshTokenAsync()` в `CookieAuthenticationEvents.OnSigningOut` отзывает refresh-токен в Keycloak.

## 4. Наш код против ATM

| Наш компонент (C0) | Что с ним при ATM |
|---|---|
| `KeycloakTokenClient` (typed HttpClient, классификация Success/Rejected/Unavailable) | Транспорт заменяет `IOpenIdConnectUserTokenEndpoint`. Классификацию ATM не делает: остаётся своей, но уже как разбор `FailedResult.Error` |
| `SessionTokenWriter` (StoreTokens + ResetWindow + SignIn) | Запись и SignIn делает `IUserTokenStore`. `ResetWindow` переезжает в `CookieAuthenticationEvents.OnSigningIn` ([§10](#10-окно-сессии-adr-0027-что-меняется)) |
| `AccessTokenExpiryHelper` и `OnTicketReceived` (минус `ClockSkewBuffer`) | Удаляются. ATM пишет `expires_at` без буфера, вместо буфера порог: `RefreshBeforeExpiration = RefreshThreshold + ClockSkewBuffer` (1:30) |
| `TokenRefreshService`: порог и решение «нужен ли refresh» | Делает `IUserTokenManager` |
| `TokenRefreshService`: замок на `bff_session_id`, double-check, backoff, SignOut | Остаётся своим, переезжает в собственный `IUserTokenRequestConcurrencyControl` и тонкий middleware ([§9](#9-предлагаемая-схема-интеграции)) |
| `TokenRefreshSemaphores` и `TokenRefreshSessionJanitor` | Можно убрать при схеме «`Lazy<Task>` + удаление в `finally`», как у ATM. Остаётся короткоживущий кэш «старый refresh → новый результат» с TTL, без уборщика |
| `TokenRefreshMiddleware` | Остаётся: вызывает `GetUserAccessTokenAsync` на каждый запрос (упреждающий refresh) и применяет политику исхода |
| `DistributedCacheTicketStore`, `SessionCookieLifetime`, `ConfigureOidcOptions` (вход, Remember Me) | Без изменений. ATM работает поверх `SaveTokens = true` и любого `SessionStore` |
| Нет | Новое: HTTP-обработчик для исходящих вызовов, revoke на выходе, DPoP (опционально), метрики `AccessTokenManagementMetrics` |

## 5. Чего ATM не делает и где он нам опасен

### 5.1. Поздний запрос с отозванным refresh-токеном (критично)

Сценарий при ротации:

1. Запросы A и B пришли почти одновременно и прочитали сессию с refresh-токеном RT1.
2. A вызвал `GetAccessTokenAsync`, создал задачу с ключом RT1 и обновил токены. Keycloak выдал RT2 и отозвал RT1. A записал RT2 в стор. Запись словаря удалена в `finally`.
3. B добрался до `GetAccessTokenAsync` позже (долгий код до вызова, медленный поток). Ключа RT1 в словаре уже нет, поэтому B создаёт новую задачу и шлёт RT1. Keycloak отвечает `invalid_grant`.

Дальнейшее зависит от того, что мы делаем с `invalid_grant`:
- если выходим из сессии (наше правило Rejected), то **здоровая сессия убита**;
- если не выходим, то вызов B уходит без токена, API отвечает 401, а следующий запрос пользователя уже прочитает RT2 и пройдёт.

В C0 этот случай закрыт: B под замком видит свежий `LastRefreshedAt` и пропускает refresh. Тот же вывод сделал автор `pdevito3/FullstackTemplate` (issue #24, закрыт 2026-10-20): ротация плюс «`invalid_grant` завершает сессию» требуют своего контроля конкуренции с перечитыванием после замка.

**Вывод:** берём ATM, только если заменяем `IUserTokenRequestConcurrencyControl` своим. Публичный интерфейс это позволяет.

### 5.2. Нет sign-out при отказе и нет backoff

- `invalid_grant` и сетевая ошибка приходят одним типом `FailedResult`. Различить их можно только по строке `Error`: OAuth-код (`invalid_grant`, `invalid_client`) или текст HTTP-ошибки или исключения от `Duende.IdentityModel`. Точный формат строки для сетевых ошибок нужно проверить тестом.
- После сбоя каждый следующий запрос снова пойдёт в Keycloak. Паузы `RefreshRetryBackoff` нет.
- Значит, политика исходов (Rejected → SignOut, Unavailable → backoff, `invalid_client` → не трогать сессию и поднять тревогу) остаётся своей.

### 5.3. Таймаут token endpoint

ATM ходит в Keycloak через `OpenIdConnectOptions.Backchannel`. По умолчанию `BackchannelTimeout` = 60 с, у нас сейчас 5 с на typed client. Если поставить `options.BackchannelTimeout = 5 с`, то он распространится и на обмен кода при входе, и на discovery. Для Keycloak в одной сети это допустимо, но решение нужно принять явно.

### 5.4. Часы

`UserAccessAccessTokenManager` сравнивает время по `TimeProvider` из DI. Но `OpenIdConnectUserTokenEndpoint` считает `Expiration = DateTimeOffset.UtcNow + expires_in` по системным часам. Для продакшна это не важно. Для наших InProcess-тестов на `FakeTimeProvider` важно: после сдвига часов вперёд токен из refresh будет выглядеть истёкшим. Выход: в тестах подменять `IOpenIdConnectUserTokenEndpoint` (публичный интерфейс) фейком на `FakeTimeProvider` вместо `FakeKeycloak` на уровне HTTP.

### 5.5. Окно cookie

`StoreTokensInAuthenticationProperties` обнуляет `IssuedUtc` и `ExpiresUtc` перед SignIn, только если `AllowRefresh == true` или `SlidingExpiration == true`. У нас скольжение выключено, поэтому SignIn от ATM сохранит старые даты и окно не сдвинется. Нужен свой хук ([§10](#10-окно-сессии-adr-0027-что-меняется)).

### 5.6. Масштабирование

Словарь single-flight живёт в процессе. Для нескольких экземпляров нужны распределённый замок и общий кэш «старый refresh → результат». Готового решения у Duende нет: это наш `IUserTokenRequestConcurrencyControl` поверх Redis.

### 5.7. Что ATM не касается вообще

401-контракт для API, CSRF, формат ошибок, `Cache-Control` у `/session`, GET `/logout`, Data Protection, предел in-memory стора.

## 6. Таблица слабых мест C0 после ATM

Нумерация по таблице `recallapp-c0-weak-spots` (п.1 дополнения C0). Все её пункты пока **не согласованы**.

| # | Пункт | ATM закрывает? |
|---|---|---|
| 1.1 | Предел и тихий отказ in-memory стора | Нет |
| 1.2, 1.3 | Брошенные записи Remember Me, вторая сессия при повторном `/login` | Нет |
| 1.4 | Ключи Data Protection | Нет |
| 1.5 | Мёртвая cookie после рестарта | Нет |
| 1.6 | Ожидание без `RequestAborted` | Частично: всё API ATM принимает `CancellationToken` |
| 1.7 | Допущение «ожидание замка < порога» | Частично: ждавшие получают результат той же задачи. Поздних (§5.1) закрывает только наш concurrency control |
| 1.8 | `invalid_client` разлогинивает всех | Нет, но `FailedResult.Error` даёт OAuth-код, по нему проще классифицировать |
| 1.9 | DeveloperExceptionPage | Нет |
| 1.11 | Ключ замка через `sub` | Исчезает вместе с `TokenRefreshService` |
| 1.13 | Распределённый замок | Нет. Точка расширения есть (`IUserTokenRequestConcurrencyControl`) |
| 2.1 | Шов исходящего вызова | **Да**: `AddUserAccessTokenHandler`, `AccessTokenRequestHandler` |
| 2.2 | §8.4, старый access после Rejected | Нет. Хуже того, обработчик ATM при неудаче шлёт запрос **без** токена. Нужна своя политика в middleware |
| 2.3 | Протухший access при Unavailable | Нет |
| 2.4 | Старый access у ждавших замок | **Да** для одновременных: они получают новый токен из общей задачи. Поздние зависят от нашего concurrency control |
| 2.5, 2.6 | Хранение обменянных токенов и очистка на выходе | Нет. `OnSigningOut` + `RevokeRefreshTokenAsync` дают место для очистки |
| 2.7 | Клиент только для refresh grant | Частично: `Duende.IdentityModel` (транзитивно) умеет `RequestTokenExchangeTokenAsync` |
| 3.x | Контракт API для клиента | Нет |

## 7. Token Exchange поверх ATM

### 7.1. Что есть в ATM

Готового Token Exchange (RFC 8693) для пользовательских токенов нет: ни в документации, ни в коде. Есть точки, на которых он строится:
- `AccessTokenRequestHandler` — публичный `DelegatingHandler` с публичным конструктором и вложенным интерфейсом `ITokenRetriever`. Класс `sealed` (проверено в тегах `atm-4.0.0` и `atm-4.3.0`), наследоваться от него нельзя: расширяем композицией, подавая свой `ITokenRetriever` в конструктор. При неудаче получения токена он шлёт запрос без токена; если для API нужен 401, нужен свой `DelegatingHandler` (~30 строк) с тем же `ITokenRetriever`;
- `IUserTokenManager` отдаёт свежий пользовательский access;
- `Duende.IdentityModel`: `HttpClient.RequestTokenExchangeTokenAsync(TokenExchangeTokenRequest)`;
- `HybridCache` уже подключается ATM для client credentials.

### 7.2. Схема

```
SPA → BFF /api/{service}/** → HttpClient сервиса
   → AccessTokenRequestHandler(ITokenRetriever = ExchangedTokenRetriever)
       1. IUserTokenManager.GetAccessTokenAsync(user)          // subject token, при нужде refresh
       2. кэш: ключ (bff_session_id, audience)                  // HybridCache, TTL ≤ exp − буфер
       3. промах: RequestTokenExchangeTokenAsync(subject_token, audience, [scope])
   → Authorization: Bearer <обменянный токен> → сервис
```

- Кэш обменянных токенов держим отдельно от тикета. Тогда их запись не делает SignIn и не сдвигает окно (вопрос 2.5 таблицы).
- Очистка на выходе: в `OnSigningOut` удалить ключи сессии из кэша и вызвать `RevokeRefreshTokenAsync`.
- Отзыв subject access в Keycloak **не** отзывает обменянный access (документация Keycloak, раздел Revocation). Короткий TTL кэша и очистка на выходе обязательны.

### 7.3. Требования Keycloak 26 (Standard Token Exchange, V2)

По документации Keycloak «Configuring and using token exchange»:
- клиент, который запрашивает обмен (`Bff.Web`), должен быть confidential, и на нём включён переключатель **Standard token exchange**;
- `aud` subject token должен содержать запрашивающего. **Исключение:** клиент обменивает токен, выданный ему самому. Наш access выдан `Bff.Web`, значит audience mapper на `Bff.Web` скорее всего не нужен; проверить на живом realm;
- `audience` только **сужает** набор аудиторий из скоупов и не добавляет новых. Целевой сервис должен попадать в токен через client scope или audience mapper. Недоступная аудитория отклоняет запрос;
- параметр `resource` не поддерживается. `requested_token_type` по умолчанию access token. Обмен не создаёт новую пользовательскую сессию.

Realm настраиваешь ты. Чек-лист войдёт в `Iam/README.md` на этапе C1.

## 8. Ветка «если добавим Duende.BFF»

| Пункт | Факт |
|---|---|
| Лицензия | Коммерческая. Для продакшна нужна лицензия Duende (редакции Starter и Enterprise). Бесплатный Community Edition находится у IdentityServer; бесплатной редакции BFF в источниках не нашёл. Разработка и тесты без лицензии работают в trial-режиме |
| Что дал бы сверх ATM | 401 вместо 302 для API-эндпоинтов, обязательный заголовок `X-CSRF`, эндпоинты `/bff/login`, `/bff/user`, `/bff/logout`, server-side sessions с EF, back-channel logout, прокси на YARP с подстановкой токена |
| Что не дал бы | Распределённый замок refresh при ротации. Автор `pdevito3/FullstackTemplate` уходит с Duende.BFF на ATM и пишет этот замок сам (issue #24) |
| Рекомендация | Не подключать. Нужное нам (401-контракт, `X-CSRF`, `/session` как `/bff/user`) — это 100–200 строк своего кода. Duende.BFF берём как источник паттернов |

## 9. Предлагаемая схема интеграции

```csharp
// Program.cs (черновик)
builder.Services.AddOpenIdConnectAccessTokenManagement(o =>
{
    // Вместо ClockSkewBuffer: refresh за 1:30 до истечения.
    o.RefreshBeforeExpiration = tokenLifetime.RefreshThreshold + tokenLifetime.ClockSkewBuffer;
    // По умолчанию ATM шлёт секрет в заголовке Basic; KeycloakTokenClient сейчас шлёт его в теле.
    o.ClientCredentialStyle = ClientCredentialStyle.PostBody;
});

// Свой контроль конкуренции: ключ bff_session_id, перечитывание после ротации, backoff.
builder.Services.AddSingleton<IUserTokenRequestConcurrencyControl, SessionRefreshConcurrencyControl>();

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure(o =>
    {
        o.Events.OnSigningIn = ctx => { /* ResetWindow(ctx.Properties): окно двигает любой SignIn */ };
        o.Events.OnSigningOut = async ctx => { await ctx.HttpContext.RevokeRefreshTokenAsync(); /* + очистка кэшей сессии */ };
    });

// Упреждающий refresh и политика исхода на каждый запрос.
app.UseAuthentication();
app.UseMiddleware<SessionTokenMiddleware>(); // GetUserAccessTokenAsync → Success | Rejected → SignOut + аноним/401 | Unavailable → дальше
app.UseAuthorization();

// Исходящие вызовы (C1).
builder.Services.AddHttpClient<RecallApiClient>(...).AddUserAccessTokenHandler();
```

`SessionRefreshConcurrencyControl` (наш, ~100 строк):

1. Достаёт `bff_session_id` из properties текущего запроса через `IHttpContextAccessor`.
2. Ищет в кэше ротаций по хэшу пришедшего refresh-токена. Нашёл: этот токен уже обменян, возвращает сохранённый новый `UserToken` и в Keycloak не ходит. Это закрывает §5.1. TTL записи — порядка `RefreshBeforeExpiration`.
3. Сессия в backoff после Unavailable: сразу `FailedResult("temporarily_unavailable")`.
4. Иначе single-flight `Lazy<Task>` по `bff_session_id` с удалением в `finally`, как у ATM, но ключ сессионный.
5. Внутри задачи вызывает `tokenRetriever()` (ATM сам делает POST и запись). При успехе кладёт «старый RT → новый `UserToken`» в кэш ротаций. При Unavailable ставит backoff.
6. Распределённый вариант позже: кэш ротаций в Redis (`HybridCache`/`IDistributedCache`) плюс распределённый замок. Интерфейс тот же.

`SessionTokenMiddleware` заменяет `TokenRefreshMiddleware`:
- `Success`: дальше;
- `invalid_grant`: `SignOutAsync`, `context.User` в аноним (§8.4), для `/api/**` 401 (контракт 3.1 не согласован);
- `invalid_client` и прочие OAuth-коды конфигурации: сессию не трогаем, `LogError`;
- сеть, 5xx, backoff: дальше без изменений, как Unavailable сейчас.

## 10. Окно сессии (ADR 0027): что меняется

Сейчас окно сдвигает только наш refresh (`SessionTokenWriter` → `ResetWindow`). При ATM SignIn делает `AuthenticationSessionUserAccessTokenStore`, и в него наш вариант автоматически не встроится. Варианты:

| Вариант | Как | Плюсы | Минусы |
|---|---|---|---|
| **А. `OnSigningIn` → `ResetWindow`** (рекомендую) | Любой SignIn на cookie-схеме начинает окно заново | Правило из §8.5 («каждый SignIn сдвигает окно») становится механизмом фреймворка, а не соглашением. Вход тоже проходит через это событие, `Apply` и `ResetWindow` дают одинаковый результат | Если в будущем появится SignIn не из-за refresh (например, обновление claims), он тоже сдвинет окно. Для нас это допустимо |
| Б. `AllowRefresh = true` на входе | ATM сам обнулит даты, cookie-handler выставит `now + ExpireTimeSpan` | Ничего не пишем | `ExpireTimeSpan` один на схему, а окно Remember Me 30 дней против 30 мин. Ломает режимы. Отклонить |
| В. Свой `IUserTokenStore` (обёртка над стандартным) | Перед SignIn вызывает `ResetWindow` | Явно и локально | Стандартный store `internal`, обёртку пришлось бы писать с нуля (~80 строк). Дублирует ATM |

Упреждающий refresh в middleware оставляем, ленивый не берём. ATM сам обновляет токен лениво, при исходящем вызове. Тогда окно двигалось бы только при вызовах API, а при опросе `/session` Keycloak SSO Idle истёк бы раньше cookie. Middleware с `GetUserAccessTokenAsync` на каждый запрос сохраняет семантику ADR 0027: активность пользователя продлевает и cookie, и сессию Keycloak тем же событием.

## 11. План миграции

Каждый шаг — отдельный коммит (fix и refactor раздельно).

1. **ADR.** Решение «ATM вместо своего транспорта refresh» и пересмотр механизма окна в ADR 0027 (вариант А).
2. **Пакет.** `Duende.AccessTokenManagement.OpenIdConnect` 4.3.0 в `Host.csproj`. Сборка, конфликтов `Microsoft.IdentityModel.*` нет (проверить).
3. **Шов тестов.** В `BffApp` подменить `IOpenIdConnectUserTokenEndpoint` фейком на `FakeTimeProvider` (§5.4). `FakeKeycloak` остаётся для unit-тестов, пока жив `KeycloakTokenClient`.
4. **Concurrency control + middleware.** `SessionRefreshConcurrencyControl`, `SessionTokenMiddleware`, `OnSigningIn` → `ResetWindow`, `RefreshBeforeExpiration` = 1:30, `BackchannelTimeout`.
5. **Удаление.** `KeycloakTokenClient`, `SessionTokenWriter`, `AccessTokenExpiryHelper`, `OnTicketReceived`, `TokenRefreshService`, `TokenRefreshSemaphores`, `TokenRefreshSessionJanitor` и их unit-тесты.
6. **Тесты.**
   - InProcess-сценарии C0 переносятся как есть (они проверяют поведение через HTTP).
   - Новый тест на §5.1: поздний запрос со старым RT не идёт в Keycloak и не разлогинивает.
   - Keycloak-категория: проверить, как обработан `invalid_grant` после ротации.
   - Мутационная проверка: убрать кэш ротаций, тест §5.1 должен упасть.
7. **Выход.** `OnSigningOut` → `RevokeRefreshTokenAsync`. Проверить, что Keycloak после revocation закрывает и SSO-сессию либо её сохраняет.
8. **Доки.** `bff-session-design.md`: §2, §5, диаграммы 02, 05, 06, 08. Ручные сценарии 3 и 9 прогнать заново.

Token Exchange (§7) — отдельный этап C1 после миграции.

## 12. Решения, нужные от владельца

1. Принимаем ли ATM при условии своего `IUserTokenRequestConcurrencyControl` (§5.1)? Без него брать нельзя.
2. Механизм окна: вариант А (`OnSigningIn`)?
3. `BackchannelTimeout = 5 с` для всего OIDC-backchannel или отдельный `HttpClient` для ATM? Второе требует проверки, можно ли его подать.
4. `OnSigningOut` → `RevokeRefreshTokenAsync`: включаем? Это лишний запрос к Keycloak на каждый выход, зато refresh-токен гарантированно мёртв.
5. Порядок работ: сначала миграция на ATM, потом остальной п.1 дополнения C0 (401, ProblemDetails, CSRF)? Или наоборот?

## 13. Источники

- Исходники ATM: [DuendeSoftware/foss, access-token-management](https://github.com/DuendeSoftware/foss/tree/main/access-token-management).
- [Документация ATM](https://docs.duendesoftware.com/accesstokenmanagement/).
- [NuGet: Duende.AccessTokenManagement.OpenIdConnect](https://www.nuget.org/packages/Duende.AccessTokenManagement.OpenIdConnect/)
- [foss#360: single-flight с разными resource](https://github.com/DuendeSoftware/foss/issues/360)
- [pdevito3/FullstackTemplate#24: замок refresh между экземплярами BFF](https://github.com/pdevito3/FullstackTemplate/issues/24)
- [DuendeArchive/Support#966: дубли refresh при параллельных вызовах](https://github.com/DuendeArchive/Support/issues/966)
- [Keycloak: Configuring and using token exchange](https://www.keycloak.org/securing-apps/token-exchange), [Standard Token Exchange в Keycloak 26.2](https://www.keycloak.org/2025/05/standard-token-exchange-kc-26-2)
- [Damien Bod: delegated token exchange в ASP.NET Core](https://damienbod.com/2025/02/10/asp-net-core-delegated-oauth-token-exchange-access-token-management/) — пример своего кэша, без ATM
- [Duende: trial mode и лицензирование](https://docs.duendesoftware.com/trial-mode), [Duende license](https://duendesoftware.com/license)
