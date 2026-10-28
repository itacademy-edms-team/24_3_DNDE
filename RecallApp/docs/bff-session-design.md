# BFF-сессия: как устроены вход, аутентификация запроса и обновление токенов (C0)

Документ описывает текущую реализацию `Bff/Bff.Web/Host` на ветке `acct-01`. Его задача: дать картину «кто кого вызывает, что куда пишет и почему», а заодно служить ревью дизайна перед рефакторингом и тестами.

Решения, на которые опирается код: ADR 0002 (BFF), 0021 (серверное хранилище сессии), 0025 и 0027. Ручные сценарии проверки лежат в [bff-manual-tests.md](bff-manual-tests.md).

## Содержание

1. [Где живёт состояние](#1-где-живёт-состояние)
2. [Роли и ответственность](#2-роли-и-ответственность)
3. [Вход: три HTTP-запроса](#3-вход-три-http-запроса)
4. [AuthenticationMiddleware: от входа до выхода](#4-authenticationmiddleware-от-входа-до-выхода)
5. [TokenRefreshMiddleware: от входа до выхода](#5-tokenrefreshmiddleware-от-входа-до-выхода)
6. [Выход из аккаунта](#6-выход-из-аккаунта)
7. [Жизнь сессии и окно cookie во времени](#7-жизнь-сессии-и-окно-cookie-во-времени)
8. [Трудно объяснить: кандидаты на рефакторинг](#8-трудно-объяснить-кандидаты-на-рефакторинг)

Схемы лежат отдельными файлами в [diagrams/](diagrams/), все с префиксом `bff-session -`.

## 1. Где живёт состояние

### 1.1. Четыре области

Главный вопрос к любому куску состояния: переживает ли он запрос.

| Область | Сколько живёт | Кто видит |
|---|---|---|
| **Браузер** | по правилам cookie | только этот браузер |
| **Процесс BFF** | пока жив процесс, или до собственного срока записи | все запросы всех пользователей |
| **Запрос** | от входа запроса в конвейер до отправки ответа | только этот `HttpContext` |
| **Keycloak** | SSO Idle / Max режима | Keycloak |

Слово «в памяти» встречается в двух областях, и их легко спутать.

Стор сессии сейчас in-memory (`AddDistributedMemoryCache`). Это **память процесса**: одна запись на сессию, её видит любой запрос с этой cookie, сегодня, через минуту, через час.

Тикет, с которым работает код во время запроса, тоже лежит в памяти. Но это **копия, созданная для одного запроса**: обработчик схемы `Cookies` десериализует её из стора, держит в своих полях и выбрасывает вместе с запросом. Следующий запрос получит новый обработчик и новую копию.

### 1.2. Между запросами

| Хранилище | Область | Что лежит | Кто пишет | Кто читает | Срок | Почему там |
|---|---|---|---|---|---|---|
| Cookie `recall_bff` | браузер | зашифрованный мини-тикет с одним claim `…Cookies-SessionId` = ключ стора `bff-session:<guid>` | `CookieAuthenticationHandler` (SignIn, SignOut) | `CookieAuthenticationHandler`, один раз за запрос | без Remember Me до закрытия браузера, с ним до `ExpiresUtc` | токены в браузер не попадают (ADR 0002), cookie несёт только ссылку |
| `ITicketStore` → `DistributedCacheTicketStore` → `IDistributedCache` | процесс (singleton) | **сериализованный** `AuthenticationTicket`: principal (claims) + `AuthenticationProperties` (токены, `bff_session_id`, `bff_remember_me`, `IssuedUtc`, `ExpiresUtc`, `IsPersistent`) | `CookieAuthenticationHandler`: `StoreAsync` на входе, `RenewAsync` на SignIn (refresh), `RemoveAsync` на SignOut и истечении | `CookieAuthenticationHandler`: `RetrieveAsync`, один раз за запрос | абсолютный срок записи = `ExpiresUtc` | серверная сессия (ADR 0021). Запись перезаписывается целиком, последняя запись побеждает |
| `TokenRefreshSemaphores` | процесс (singleton) | словари по ключу `bff_session_id`: `SemaphoreSlim`, `LastRefreshedAt`, `RetryNotBefore` | `TokenRefreshService`, `TokenRefreshSessionJanitor` | `TokenRefreshService` | до вытеснения уборщиком (простой > `SessionLockIdle`) или ленивым trim | см. п. 1.4 |
| Сессия Keycloak (БД `iam-db`) | Keycloak | SSO-сессия, цепочка refresh-токенов | Keycloak | Keycloak | SSO Idle / Max режима | источник истины: BFF-сессия должна совпадать с ней по состоянию и сроку |

### 1.3. Внутри одного запроса

| Что | Область | Что лежит | Кто создаёт и меняет | Кто читает |
|---|---|---|---|---|
| Экземпляр `CookieAuthenticationHandler` | запрос | поля: `_readCookieTask` и `_authenticateTask` (запомненный результат чтения cookie и стора), `_sessionKey` (ключ стора), `_signInCalled`, `_signOutCalled`. Поля встроенного продления (`_shouldRefresh`, `_refreshTicket`) при `SlidingExpiration = false` не заполняются | `IAuthenticationHandlerProvider` создаёт его при первом обращении к схеме в этом запросе; поля заполняет сам handler | все вызовы `HttpContext.AuthenticateAsync/SignInAsync/SignOutAsync("Cookies")` в этом запросе |
| Объект тикета (`AuthenticationTicket` + `AuthenticationProperties`) | запрос | десериализованная копия записи стора | handler при чтении; наш код меняет его напрямую (`StoreTokens`, `ResetWindow`) | `TokenRefreshService`, `GetTokenAsync`, контроллеры |
| `HttpContext.User` | запрос | principal из тикета | `AuthenticationMiddleware` | авторизация, контроллеры |

### 1.4. Как копия запроса связана с записью стора

1. **Чтение.** Первый `AuthenticateAsync("Cookies")` в запросе (его делает `AuthenticationMiddleware`) вызывает `RetrieveAsync`. Handler запоминает результат. Все следующие `AuthenticateAsync` в этом запросе возвращают **тот же** результат с **тем же** объектом `AuthenticationProperties`, в стор не ходят.
2. **Изменение.** Код меняет только копию. Пока никто не позвал сохранение, стор об изменениях не знает.
3. **Сохранение.** Копия попадает в стор только через явный `SignInAsync("Cookies")` (сразу `RenewAsync` по `_sessionKey`). У нас его вызывает только refresh под замком сессии. Сохранение перезаписывает запись целиком.

   До 2026-10-28 был второй путь, встроенное продление в `FinishResponseAsync`. Он отключён (`SlidingExpiration = false`), причина в [п. 8.1](#81-параллельное-продление-затирает-новые-токены).
4. **Параллельные запросы.** У каждого запроса своя копия, прочитанная в момент его входа в конвейер. Запрос B не видит, что запрос A только что сохранил новые токены. Если бы B потом сохранил свою копию, он затёр бы работу A. Сейчас B без собственного refresh в стор не пишет, а до refresh его не пускает double-check (п. 5.1, строка 9).
5. **Следующий запрос** снова читает стор и получает последнюю сохранённую версию.

Отсюда причина, по которой `TokenRefreshSemaphores` лежат в процессе, а не в тикете. Замок должен быть **общим** для параллельных запросов одной сессии, а тикет у каждого из них свой. К тому же `SemaphoreSlim` не сериализуется.

### 1.5. Временные хранилища входа

Фреймворк заводит их только на время входа:
- **параметр `state`** в URL Keycloak: зашифрованные `AuthenticationProperties` запроса `/login` (`RedirectUri`, PKCE `code_verifier`, correlation id);
- **cookie `.AspNetCore.Correlation.*`**: парная к correlation id в `state`, защищает callback от CSRF;
- **cookie `.AspNetCore.OpenIdConnect.Nonce.*`**: nonce, которым проверяется id_token (защита от replay). В properties nonce не кладётся.

Нам они важны тем, что `AuthenticationProperties` переезжают из запроса `/login` в запрос `/signin-oidc` через `state`.

## 2. Роли и ответственность

Схема: [bff-session - 01 компоненты и хранилища.puml](diagrams/bff-session%20-%2001%20компоненты%20и%20хранилища.puml).

### 2.1. Фреймворк

| Объект | Время жизни | Роль | Где включается |
|---|---|---|---|
| `AuthenticationMiddleware` | один на приложение | диспетчер: отдаёт callback-запросы OIDC-обработчику, для остальных вызывает аутентификацию схемы по умолчанию и кладёт результат в `HttpContext.User` | `app.UseAuthentication()` |
| `IAuthenticationService` | scoped (запрос) | фасад над схемами: `AuthenticateAsync`, `ChallengeAsync`, `SignInAsync`, `SignOutAsync` по имени схемы. Через него идут все `HttpContext.XxxAsync` | `AddAuthentication` |
| `IAuthenticationHandlerProvider` | scoped (запрос) | создаёт обработчик схемы при первом обращении в запросе и отдаёт **тот же** экземпляр на все последующие. На этом держится «стор читается один раз за запрос» | `AddAuthentication` |
| `CookieAuthenticationHandler` (схема `Cookies`, по умолчанию) | новый на каждый запрос | владелец cookie и стора: читает cookie, достаёт тикет из стора, проверяет срок, пишет и удаляет cookie | `AddCookie` + `SessionStore` в `Program.cs` |
| `OpenIdConnectHandler` (схема `OpenIdConnect`, challenge по умолчанию) | новый на каждый запрос | протокол OIDC: редирект на Keycloak, обработка `/signin-oidc` (обмен кода, проверка id_token, userinfo), передача principal в схему `Cookies` через `SignInAsync`; logout в Keycloak | `AddOpenIdConnect` + `ConfigureOidcOptions` |
| `TicketDataFormat` (Data Protection) | singleton (в опциях) | шифрует и расшифровывает содержимое cookie | ставится фреймворком в `PostConfigure` |

Что скрыто и для понимания не нужно:
- `IAuthenticationSchemeProvider` знает схемы и схемы по умолчанию.
- `ChunkingCookieManager` режет большие cookie на куски. Наша cookie маленькая, поэтому кусков нет.
- `IOptionsMonitor` / `PostConfigure` собирает опции.
- `ConfigurationManager` скачивает discovery-документ Keycloak.

### 2.2. Наш код

| Объект | Время жизни | Роль | Пишет в | Читает из |
|---|---|---|---|---|
| `AuthController` | на запрос | точки входа: `/login` (Challenge OIDC), `/logout` (SignOut обеих схем), `/session` (состояние) | (через фреймворк) | `HttpContext.User`, `GetTokenAsync("access_token")` |
| `ConfigureOidcOptions` | singleton (опции) | настройка OIDC-клиента и события входа. `OnTokenValidated`: стабильный `bff_session_id`, определение режима Remember Me по `refresh_expires_in`. `OnTicketReceived`: `expires_at` по правилу `AccessTokenExpiryHelper` | объект properties входа | ответ token-endpoint, токены от `SaveTokens` |
| `SessionCookieLifetime` | static | политика окна cookie: `Apply` на входе (режим + окно), `ResetWindow` на token refresh | объект properties | `bff_remember_me`, `SsoSessionOptions` |
| `DistributedCacheTicketStore` | singleton | реализация `ITicketStore`: сериализация тикета в `IDistributedCache` со сроком = `ExpiresUtc` | `IDistributedCache` | `IDistributedCache` |
| `TokenRefreshMiddleware` | scoped | тонкая обёртка: вызывает сервис refresh для не-WebSocket запросов, потом `next` | | |
| `TokenRefreshService` | scoped | координатор упреждающего refresh: порог, замок, double-check, backoff, SignOut при отказе (подробно в [разделе 5](#5-tokenrefreshmiddleware-от-входа-до-выхода)) | `TokenRefreshSemaphores`; через `SignOutAsync` стор и cookie | копия тикета запроса, `TokenRefreshSemaphores` |
| `KeycloakTokenClient` | typed `HttpClient` (transient) | запрос `grant_type=refresh_token` к token-endpoint и классификация ответа в Success / Rejected / Unavailable. Сессию не трогает | | ответ Keycloak |
| `SessionTokenWriter` | singleton | запись токенов после refresh в копию тикета и сохранение сессии. Единственное место ручного SignIn после входа: сохранение всегда сдвигает окно (`ResetWindow`) | копия тикета (`StoreTokens`, `ResetWindow`), через `SignInAsync` стор и cookie | копия тикета запроса |
| `AccessTokenExpiryHelper` | static | единое правило `expires_at` = срок access − `ClockSkewBuffer`, формат и разбор | | |
| `TokenRefreshSemaphores` | singleton | общее для всех запросов состояние сериализации refresh по `bff_session_id` | | |
| `TokenRefreshSessionJanitor` | hosted (singleton) | раз в 5 минут вытесняет записи замков простаивающих сессий | `TokenRefreshSemaphores` | `TokenRefreshSemaphores` |

### 2.3. Кто кого вызывает и зачем

| Вызывающий | Вызываемый | Действие | Результат | Причина |
|---|---|---|---|---|
| `AuthController.Login` | `OpenIdConnectHandler` | `Challenge` | 302 на Keycloak, `RedirectUri` в `state` | страницы входа живут в Keycloak (ADR 0023) |
| `OpenIdConnectHandler` | `OnTokenValidated` | событие | `bff_session_id` и режим кладутся в properties | к этому моменту id_token проверен, то есть вход точно состоится, и ещё доступен сырой ответ token-endpoint с `refresh_expires_in` (`SaveTokens` это поле не сохраняет) |
| `OnTokenValidated` | `SessionCookieLifetime.Apply` | вызов | `IsPersistent`, `bff_remember_me`, окно | cookie и стор должны жить столько же, сколько SSO-сессия режима (ADR 0027) |
| `OpenIdConnectHandler` | `OnTicketReceived` | событие | из `expires_at` вычтен `ClockSkewBuffer` | `SaveTokens` уже записал токены, а тикет ещё не сохранён. Правило срока то же, что на refresh |
| `OpenIdConnectHandler` | `CookieAuthenticationHandler` | `SignInAsync(Cookies)` | тикет в стор (`StoreAsync`), ключ в cookie | OIDC-схема сама сессию не хранит, она передаёт её схеме `Cookies` |
| `AuthenticationMiddleware` | `CookieAuthenticationHandler` | `AuthenticateAsync` | копия тикета из стора, `HttpContext.User` | каждому запросу нужен пользователь. Стор читается здесь, один раз за запрос; handler запоминает результат для всех последующих вызовов в этом запросе |
| `TokenRefreshService` | `KeycloakTokenClient` | `RefreshAsync(refresh_token)` | исход и новые токены | HTTP и разбор ответа отделены от координации: тестируются и меняются отдельно |
| `TokenRefreshService` | `SessionTokenWriter` | `StoreRefreshedTokensAsync` при Success | новые токены в копии и в сторе | запись сессии собрана в одном месте вместе со сдвигом окна |
| `SessionTokenWriter` | `SessionCookieLifetime.ResetWindow` | вызов перед SignIn | новое окно в копии | встроенного продления нет, это единственное место, где окно сдвигается после входа. Без `ResetWindow` SignIn запишет старые даты |
| `SessionTokenWriter` | `CookieAuthenticationHandler` | `SignInAsync(Cookies)` с изменённой копией properties | `RenewAsync` по тому же ключу, новая cookie | без этого вызова новые токены остались бы только в копии запроса. Ключ прежний, поэтому cookie параллельных запросов остаётся валидной |
| `TokenRefreshService` | `CookieAuthenticationHandler` | `SignOutAsync(Cookies)` | запись стора и cookie удалены | Keycloak отказал окончательно, сессии больше нет |

## 3. Вход: три HTTP-запроса

Вход не происходит внутри одного middleware. Он растянут на три запроса браузера, и состояние между ними переносится в зашифрованном параметре `state` и временных cookie (п. 1.5).

Схема: [bff-session - 02 вход.puml](diagrams/bff-session%20-%2002%20вход.puml).

**Запрос 1, `GET /login`.** Проходит конвейер как обычный запрос. У нового пользователя cookie нет: `AuthenticationMiddleware` оставляет его анонимным, `TokenRefreshMiddleware` пропускает. Если пользователь уже вошёл и снова открыл `/login`, запрос аутентифицирован и проходит обычным путём, включая дерево решений refresh (раздел 5). Затем контроллер вызывает `Challenge` для схемы OIDC. `OpenIdConnectHandler`:
- кладёт в `AuthenticationProperties` адрес возврата, PKCE `code_verifier` и correlation id;
- ставит nonce-cookie и correlation-cookie;
- шифрует properties в `state` и отвечает 302 на Keycloak.

**Между запросами** пользователь вводит логин и пароль на странице Keycloak и при желании ставит Remember Me. Keycloak отвечает 302 на `/signin-oidc?code&state`.

**Запрос 3, `GET /signin-oidc`.** Его целиком забирает `OpenIdConnectHandler` прямо внутри `AuthenticationMiddleware`. Порядок:
1. Расшифровать `state` в properties, сверить correlation id с correlation-cookie.
2. Обменять `code` + `code_verifier` + секрет клиента на токены (back-channel к Keycloak).
3. Проверить id_token (подпись, nonce из nonce-cookie).
4. **`OnTokenValidated` (наш код).** Записать `bff_session_id = новый GUID`. Сравнить `refresh_expires_in` с `RememberMeThreshold` (1 день): больше значит Remember Me. Вызвать `SessionCookieLifetime.Apply`: `IsPersistent`, маркер `bff_remember_me`, `IssuedUtc = now`, `ExpiresUtc = now + SSO Idle режима` (30 мин или 30 дней).
5. `SaveTokens`: токены и `expires_at = now + expires_in` в properties.
6. Userinfo, claims.
7. `RemoteAuthenticationHandler` вынимает `RedirectUri` из properties (поэтому позже, на refresh, повторный SignIn не делает редирект), затем вызывает событие `TicketReceived`. **`OnTicketReceived` (наш код)** вычитает из `expires_at` `ClockSkewBuffer`, чтобы срок считался по тому же правилу, что и на refresh (`AccessTokenExpiry`).
8. `SignInAsync(Cookies)`: `StoreAsync` создаёт ключ `bff-session:<guid>`, кладёт тикет в стор со сроком `ExpiresUtc`, ключ шифруется в cookie `recall_bff`. Атрибут `Expires` у cookie ставится только при `IsPersistent`.
9. 302 на адрес возврата.

Запрос 3 до `TokenRefreshMiddleware` и контроллера не доходит: `AuthenticationMiddleware` возвращается, не вызывая `next`.

## 4. AuthenticationMiddleware: от входа до выхода

### 4.1. Развилка

Схема: [bff-session - 03 развилка AuthenticationMiddleware.puml](diagrams/bff-session%20-%2003%20развилка%20AuthenticationMiddleware.puml).

**Начальное состояние.** Запрос с путём, заголовками и, возможно, cookie `recall_bff`. `HttpContext.User` пустой (анонимный principal). Обработчиков схем для этого запроса ещё нет.

**Шаг 0.** В `Features` кладётся `IAuthenticationFeature` с исходными `Path` и `PathBase`. Он нужен обработчикам схем для адресов возврата.

**Шаг 1. Обработчики запросов.** Middleware перебирает схемы, чьи обработчики реализуют `IAuthenticationRequestHandler`. У нас такая одна, `OpenIdConnect`. Её `HandleRequestAsync` возвращает `true` только на `/signin-oidc` и `/signout-callback-oidc` (и на `RemoteSignOutPath`, который мы не используем). Тогда middleware сразу выходит. Ответ (302) уже сформирован, дальше по конвейеру запрос не идёт.

**Шаг 2. Схема по умолчанию.** Для всех остальных запросов вызывается `context.AuthenticateAsync("Cookies")`. Что внутри, разобрано в п. 4.2. По результату:
- есть `Principal`, значит он записывается в `HttpContext.User`;
- `Succeeded`, значит в `Features` кладутся `IAuthenticateResultFeature` и `IHttpAuthenticationFeature`.

**Конечное состояние (выход через `next`).**
- `HttpContext.User`: пользователь или аноним.
- Для этого запроса создан экземпляр `CookieAuthenticationHandler`. В его полях лежит копия тикета, прочитанная из стора, и запомненный результат аутентификации. Повторные `AuthenticateAsync("Cookies")` в этом запросе вернут их же, без обращения к стору (п. 4.3).
- Зарегистрирован `Response.OnStarting(FinishResponseAsync)`. При `SlidingExpiration = false` он ничего не делает: продление не планируется.

### 4.2. Обычный запрос: cookie, стор, User

Схема: [bff-session - 04 обычный запрос.puml](diagrams/bff-session%20-%2004%20обычный%20запрос.puml).

При создании обработчика (`InitializeHandlerAsync`, ещё до аутентификации) регистрируется `Response.OnStarting(FinishResponseAsync)`. Затем `CookieAuthenticationHandler.AuthenticateAsync`:

1. Прочитать cookie `recall_bff`. Нет cookie, значит `NoResult` и пользователь анонимный.
2. `TicketDataFormat.Unprotect`: получить мини-тикет с ключом стора. Не расшифровалось (сменились ключи Data Protection), значит `Fail`.
3. `ITicketStore.RetrieveAsync(ключ)`: десериализовать запись стора в копию для этого запроса. Записи нет, значит `Fail`, пользователь анонимный, а **cookie в браузере не удаляется**. Так бывает после рестарта BFF, потому что стор сейчас в памяти процесса.
4. `ExpiresUtc < now`: удалить запись стора, `Fail`. Обычно до этого не доходит, потому что запись стора сама истекает в `ExpiresUtc`.
5. `CheckForRefreshAsync` (встроенное скольжение) пропускается: `SlidingExpiration = false`.
6. Событие `ValidatePrincipal` (у нас не задано).
7. `Success(ticket)`. Результат запоминается в handler-е.

Обычный запрос без refresh ничего не пишет ни в стор, ни в cookie. `Set-Cookie` появляется только на входе, на refresh и на выходе. Окно сдвигает только refresh (`ResetWindow`), отсюда правило из `SessionCookieLifetime`: любой ручной SignIn сам отвечает за окно.

Как работало встроенное скольжение и почему оно отключено, описано в [п. 8.1](#81-параллельное-продление-затирает-новые-токены).

### 4.3. Один запрос: один handler, одно чтение стора

| Уровень | Что запоминается | Сколько живёт | Следствие |
|---|---|---|---|
| `IAuthenticationHandlerProvider` (scoped) | экземпляр обработчика на схему | запрос | все `AuthenticateAsync("Cookies")` в запросе идут в один объект |
| `CookieAuthenticationHandler._readCookieTask` | результат чтения cookie и стора | запрос | `RetrieveAsync` вызывается один раз за запрос |
| `AuthenticationHandler._authenticateTask` | `AuthenticateResult` | запрос | повторный `AuthenticateAsync` возвращает тот же результат, **с тем же объектом `AuthenticationProperties`** |

Между запросами ничего из этого не переносится. Следующий HTTP-запрос получает новый provider, новый handler и снова вызывает `RetrieveAsync`.

Третья строка важна для раздела 5: `SessionTokenWriter` меняет этот же объект properties, поэтому всё, что ниже по конвейеру читает properties через `AuthenticateAsync` / `GetTokenAsync`, видит уже новые токены. См. [п. 8.2](#82-мостик-httpcontextitems-не-нужен-удалён).

## 5. TokenRefreshMiddleware: от входа до выхода

**Начальное состояние.** `HttpContext.User` заполнен (или аноним). Экземпляр `CookieAuthenticationHandler` этого запроса держит копию тикета и запомненный результат аутентификации.

**Назначение.** Обновить access-токен до того, как он истечёт, чтобы следующий код (сейчас `/session`, в C1 прокси к API) работал со свежим токеном. Refresh упреждающий: срабатывает, когда до `expires_at` осталось не больше `RefreshThreshold` (1 мин).

Роли: `TokenRefreshService` решает и координирует, `KeycloakTokenClient` ходит в Keycloak, `SessionTokenWriter` сохраняет результат в сессию.

### 5.1. Дерево решений

Схема: [bff-session - 05 дерево решений refresh.puml](diagrams/bff-session%20-%2005%20дерево%20решений%20refresh.puml).

Проверки идут от дешёвых к дорогим, и каждая может отпустить запрос дальше без refresh.

| # | Проверка | Если сработала | Зачем |
|---|---|---|---|
| 1 | WebSocket-запрос | пропуск | у WebSocket нет нормального ответа для `Set-Cookie` |
| 2 | `User` не аутентифицирован | пропуск | нечего обновлять |
| 3 | `AuthenticateAsync(Cookies)` | | получить копию properties. Стор повторно не читается: возвращается результат, который `AuthenticationMiddleware` получил в этом же запросе (п. 4.3) |
| 4 | нет `bff_session_id` | fallback на `sub`, warning | старые cookie до появления маркера |
| 5 | нет или не парсится `expires_at` | пропуск, warning | |
| 6 | `expires_at − now > RefreshThreshold` | пропуск | токен свежий, это основной путь |
| 7 | `RetryNotBefore[сессия] > now` | пропуск | пауза после сбоя Keycloak, без ожидания замка |
| 8 | ждать `SemaphoreSlim` сессии | | один refresh на сессию одновременно: refresh-токен одноразовый (Revoke Refresh Token = On) |
| 9 | `now − LastRefreshedAt < RefreshThreshold` | пропуск (double-check) | пока ждали замок, другой запрос уже обновил |
| 10 | снова `RetryNotBefore` | пропуск | пока ждали замок, другой запрос получил сбой |
| 11 | нет `refresh_token` | `SignOutAsync` | обновить нечем |
| 12 | `KeycloakTokenClient.RefreshAsync` | Success / Rejected / Unavailable | |

Строки 8–10 читают `TokenRefreshSemaphores`, то есть общее состояние процесса. Только так запрос узнаёт, что параллельный запрос уже обновил токены: в его собственной копии тикета новых токенов нет.

### 5.2. Три исхода

Схема: [bff-session - 06 refresh исходы.puml](diagrams/bff-session%20-%2006%20refresh%20исходы.puml).

| Исход | Когда | Что меняется | Сессия |
|---|---|---|---|
| **Success** | 2xx и в JSON есть `access_token`, `expires_in` | `SessionTokenWriter`: в копии properties новые токены поверх сохранённых, `expires_at = now + expires_in − ClockSkewBuffer (30 с)`, `ResetWindow`; затем `SignInAsync` (`RenewAsync` по тому же ключу + `Set-Cookie`); `LastRefreshedAt = now`; `RetryNotBefore` удалён | продлена |
| **Rejected** | 400 (`invalid_grant`: refresh отозван, истёк, сессия Keycloak убита), 401 (неверный клиент или секрет) | `SignOutAsync`: запись стора удалена, cookie удаляется в ответе | завершена |
| **Unavailable** | 5xx, 429 и прочие не-2xx, таймаут HttpClient 5 с, сетевая ошибка, битый JSON, нет обязательных полей | `RetryNotBefore = now + 15 с`, warning | не тронута, повтор после паузы |

Почему refresh-ответ пишется через `SignInAsync`, а не прямо в стор. Стор знает только ключ, а ключ лежит в приватном поле обработчика `Cookies` (`_sessionKey`). `SignInAsync` при известном ключе вызывает `RenewAsync` по тому же ключу (а не `StoreAsync` с новым), поэтому cookie у параллельных запросов остаётся валидной.

Почему таймаут HttpClient 5 с. Запрос к Keycloak выполняется под замком сессии, остальные запросы этой сессии ждут. Таймаут ограничивает их ожидание, а backoff (п. 5.1, строки 7 и 10) не даёт каждому следующему запросу ждать снова. Таймаут всегда даёт Unavailable, никогда SignOut.

**Конечное состояние (выход через `next`).**
- **Success:**
  - копия properties этого запроса (тот же объект, что запомнил handler) несёт новые токены;
  - запись стора перезаписана этой копией, окно сдвинуто, `Set-Cookie` уже добавлен;
  - замок отпущен.
- **Rejected:**
  - запись стора удалена, cookie на удаление уже в ответе;
  - `HttpContext.User` в этом запросе **остаётся аутентифицированным** ([п. 8.4](#84-после-rejected-запрос-идёт-дальше-как-аутентифицированный)).
- **Unavailable или пропуск:** ничего не изменилось, кроме, возможно, `RetryNotBefore`. В стор и cookie ничего не пишется, окно не двигается.

### 5.3. Уборка `TokenRefreshSemaphores`

Записи замков в процессе копятся по одной на сессию. Их снимают два механизма:
- `TokenRefreshSessionJanitor` раз в 5 минут удаляет записи без `LastRefreshedAt` или с `LastRefreshedAt` старше `SessionLockIdle` (30 мин);
- ленивый trim после успешного refresh срабатывает, если записей больше 10 000, и снимает 100 самых старых.

Занятый замок (`CurrentCount == 0`) не удаляется. `SemaphoreSlim` не освобождается через Dispose, это делает GC, чтобы не гоняться с запросом, уже получившим ссылку.

## 6. Выход из аккаунта

`GET /logout` → `AuthController.Logout` → `SignOut(Cookies, OpenIdConnect)`:

1. `Cookies`: `RemoveAsync(ключ)` в сторе, удаление cookie `recall_bff`.
2. `OpenIdConnect`: 302 на end-session endpoint Keycloak с `id_token_hint` и `post_logout_redirect_uri = /signout-callback-oidc`. Keycloak завершает SSO-сессию.
3. Браузер приходит на `/signout-callback-oidc`. Его, как и `/signin-oidc`, забирает `OpenIdConnectHandler` на шаге 1 `AuthenticationMiddleware` и отвечает 302 на `SignedOutRedirectUri` (`/`).

Отдельной диаграммы нет: это тот же шаблон «контроллер → схема → 302 → callback, который забирает middleware», что и на диаграмме 02.

## 7. Жизнь сессии и окно cookie во времени

### 7.1. Состояния

Схема: [bff-session - 07 жизнь сессии.puml](diagrams/bff-session%20-%2007%20жизнь%20сессии.puml).

Состояние Deferred (Keycloak недоступен) сессию не завершает. Если Keycloak молчит дольше, чем живёт access-токен, access протухнет, а BFF-сессия останется жива. API в C1 начнёт отвечать 401, пока Keycloak не вернётся.

Окно cookie во время Deferred не двигается: его сдвигает только успешный refresh. Если простой Keycloak длится дольше остатка окна, сессия истечёт. Сессия Keycloak в это время тоже не продлевается, поведение согласовано.

### 7.2. Окно cookie

Схема: [bff-session - 08 окно cookie.puml](diagrams/bff-session%20-%2008%20окно%20cookie.puml).

Пример для обычного режима (SSO Idle 30 мин, access 5 мин, `RefreshThreshold` 1 мин, `ClockSkewBuffer` 30 с):

| Время | Событие | `expires_at` access | Окно cookie |
|---|---|---|---|
| 0:00 | вход | 0:00 + 5:00 − 0:30 = 4:30 | 0:00–30:00 |
| 4:00 | запрос, до истечения меньше 1:00 → refresh | 4:00 + 5:00 − 0:30 = 8:30 | 4:00–34:00 |
| 6:00 | запрос, токен свежий | 8:30 | без изменений: запрос без refresh окно не двигает |
| 12:00 | запрос, access уже истёк → refresh | 16:30 | 12:00–42:00 |
| 42:00 | запросов не было | | окно истекло, запись стора истекла, пользователь анонимный |

Окно истекает в момент «последний refresh + SSO Idle». Запросы в первые ~3,5 минуты после refresh его не вызывают и окно не двигают. Поэтому окно может истечь до ~3,5 минуты раньше, чем «последний запрос + 30 минут». Keycloak сбрасывает SSO Idle тем же событием (refresh), так что окно cookie и сессия Keycloak истекают согласованно.

## 8. Трудно объяснить: кандидаты на рефакторинг

Места, которые при описании потребовали оговорок. Возможен рефакторинг.

### 8.1. Параллельное продление затирает новые токены

**Статус:** исправлено 2026-10-28 (`SlidingExpiration = false`). До исправления баг подтверждён сценарием 9 ручных тестов, после исправления сценарий проходит. Механизм выведен из исходника `CookieAuthenticationHandler` (.NET 8). Разделы документа обновлены под исправление.

**Сценарий (до исправления).** Встроенное скольжение срабатывало, когда до конца окна оставалось меньше, чем прошло с `IssuedUtc`. Тогда handler на входе запроса клонировал копию тикета (`_refreshTicket`) и в `FinishResponseAsync` сохранял клон с новым окном в стор, если в запросе не было SignIn и SignOut.
1. Пользователь простоял больше 15, но меньше 30 минут. Затем SPA шлёт параллельно запросы A, B и C.
2. Каждый читает свою копию тикета (п. 1.4). Прошло больше, чем осталось, поэтому у всех трёх `ShouldRenew = true`, и в `_refreshTicket` клонируются **старые** токены.
3. A берёт замок и делает refresh. Keycloak выдаёт новый refresh-токен и отзывает старый (Max Reuse 0). A сохраняет новые токены (`SignInAsync` → `RenewAsync`), продление у A отменено.
4. B и C получают замок после A и пропускают refresh на double-check. `SignIn` в них не было, поэтому `FinishResponseAsync` выполняет `RenewAsync(тот же ключ, клон со старыми токенами)`. Последняя запись побеждает: в сторе снова отозванный refresh-токен.
5. Следующий refresh получает 400, исход Rejected, пользователь разлогинен.

Попутно B и C в своём запросе работают со старым access. Если он уже истёк, прокси в C1 получит 401.

**Как проверить.** См. [документ с ручными тестами, сценарий 9](bff-manual-tests.md#сценарий-9-параллельные-запросы-после-простоя-встроенное-продление-не-затирает-новые-токены).

**Исправление: встроенное скольжение отключено** (`SlidingExpiration = false` в `AddCookie`), окно двигает только `ResetWindow` на refresh.

Последствия:

| Аспект | До исправления | После |
|---|---|---|
| Что считается активностью | любой запрос, если прошло больше половины окна; плюс каждый refresh | только refresh |
| Когда истекает окно | от последнего продления или refresh | `последний refresh + SSO Idle` |
| Согласованность с Keycloak | В нашем потоке Keycloak сбрасывает SSO Idle только на обращении BFF к token-endpoint, то есть на refresh. Продление в BFF может увести cookie за пределы сессии Keycloak, тогда следующий refresh получит 400 | окно cookie и SSO Idle Keycloak сбрасываются одним событием. Это ближе к ADR 0027 |
| Потеря «хвоста» активности | | запросы в первые ~3,5 мин после refresh его не вызывают, поэтому окно истекает максимум на ~3,5 мин раньше, чем «последний запрос + 30 мин». Keycloak к этому моменту тоже считает сессию простаивающей |
| Простой Keycloak (Unavailable) | продление могло двигать окно во время простоя | окно не двигается. Простой дольше остатка окна приводит к выходу. Сессия Keycloak в это время тоже не продлевается, поведение согласовано |
| Запросы, где refresh пропущен (WebSocket, double-check) | могли продлевать окно | не продлевают. Для WebSocket это значит: долгая сессия только на WebSocket без HTTP-запросов истечёт по окну |
| Remember Me | то же, окно 30 дней | то же |
| Гонка из сценария выше | есть | нет: B и C ничего не пишут в стор |
| Код | | сделано: `SlidingExpiration = false` в `Program.cs`, комментарии там, в `SessionCookieLifetime`, `ConfigureOidcOptions` и `SessionTokenWriter`. `IssuedUtc` для продления больше не используется, но `ResetWindow` по-прежнему ставит оба поля, вреда от этого нет |
| Тесты | сценарий 7C (сдвиг `expires=` на refresh) | сценарий 9 прошёл вручную. Автотесты `Tests.Integration` (InProcess): без refresh `Set-Cookie` нет вовсе, при refresh он один; параллельные запросы при окне 6 мин дают один `Set-Cookie`, в сторе токены из refresh. Тест падает, если включить `SlidingExpiration` |

Отвергнутая альтернатива: оставить продление, но в событии `OnCheckSlidingExpiration` сравнивать копию с записью стора и отменять продление, если запись новее. Сложнее, лишнее чтение стора, а выгода только в «хвосте» до ~3,5 мин.

**Допущение.** Запрос со старой копией может перезаписать стор только через свой SignIn, то есть только сделав refresh. До refresh его не пускает double-check (`now − LastRefreshedAt < RefreshThreshold`). Защита держится, пока ожидание замка короче `RefreshThreshold` (1 мин). Сейчас ожидание ограничено таймаутом HttpClient 5 с на каждую попытку, а backoff не даёт ждавшим повторять попытку, поэтому риска нет. Если таймаут, backoff или порог поменяются, условие нужно перепроверить.

Остаётся отдельная проблема: B и C работают со старым access в своём запросе. Отключение продления её не решает, она относится к C1 (передача свежего токена от A к ожидавшим замок).

### 8.2. Мостик `HttpContext.Items` не нужен (удалён)

**Статус:** проверено 2026-10-28, мостик удалён.

Раньше refresh дополнительно клал новый access в `HttpContext.Items["bff_access_token"]`. Комментарий к нему утверждал, что `GetTokenAsync` после `SignInAsync` вернёт старый снимок. По исходнику .NET 8 выходит иначе.
- `AuthenticateResult` запоминается в handler-е на весь запрос, и `AuthenticationService` оборачивает его в новый тикет **с тем же объектом `Properties`** (п. 4.3).
- `SessionTokenWriter` меняет именно этот объект (`StoreTokens`).
- Значит, `GetTokenAsync("access_token")` ниже по конвейеру в этом же запросе вернёт новый токен.

Проверка: временный лог в `/session` сравнил `GetTokenAsync("access_token")` с `Items` в запросе, где прошёл refresh. Значения совпали, то есть контроллер получил новый токен. Второй `AuthenticateAsync` в записи токенов тоже убран: `authResult` передаётся параметром.

### 8.3. `TokenRefreshService` совмещал много ролей

**Статус:** разделено 2026-10-28 по кандидатному разбиению ниже, плюс `TimeProvider`. Роли описаны в п. 2.2.

Было в одном классе:
1. фильтр «нужно ли обновлять» (порог, парсинг `expires_at`);
2. координация по сессии (замок, double-check, backoff, ленивый trim);
3. HTTP-клиент Keycloak и классификация ответа в Success/Rejected/Unavailable;
4. запись результата в сессию (`StoreTokens`, `ResetWindow`, `SignInAsync`);
5. завершение сессии (`SignOutAsync`).

Пункты 3 и 4 тестируются отдельно и меняются по разным причинам. Кандидатное разбиение:
- `KeycloakTokenClient` (п. 3);
- `SessionTokenWriter` (п. 4);
- координатор (пп. 1–2, 5).

Плюс `TimeProvider` вместо `DateTimeOffset.UtcNow`.

### 8.4. После Rejected запрос идёт дальше как аутентифицированный

`SignOutAsync` удаляет стор и cookie, но не сбрасывает `HttpContext.User` и запомненный результат аутентификации. Контроллер в этом же запросе увидит пользователя и старые токены. Для `/session` это даёт отставание на один запрос. Для прокси к API в C1 это значит запрос с протухшим access и 401 от API. Варианты на C1:
- сбросить `context.User` в аноним после SignOut;
- сразу ответить 401 из middleware.

### 8.5. Окно держится на правиле, которое не проверяет код

**Статус:** закрыто разбиением п. 8.3. SignIn после входа делает только `SessionTokenWriter`, и его приватный `SaveSessionAsync` всегда вызывает `ResetWindow` перед SignIn. Компилятор по-прежнему не запрещает ручной `SignInAsync` в другом месте, но правило теперь записано в одном классе.

После исправления п. 8.1 окно двигает один механизм, `ResetWindow`. Он держится на правиле «любой ручной SignIn обязан звать `ResetWindow`», и это правило нигде не проверяется кодом. Кандидат на рефакторинг: один метод «сохранить сессию», который сам сдвигает окно и вызывает SignIn.

### 8.6. `expires_at` считался двумя способами

**Статус:** исправлено 2026-10-28, проверено вживую.

На входе `SaveTokens` писал `now + expires_in`, на refresh наш код писал `now + expires_in − ClockSkewBuffer`. Первый refresh после входа поэтому наступал на 30 с позже, чем последующие.

Теперь правило одно и живёт в `AccessTokenExpiry`. На refresh его применяет `SessionTokenWriter`. На входе `OnTicketReceived` вычитает буфер из значения, которое записал `SaveTokens`. `OnTokenValidated` для этого не подходит: он срабатывает до `SaveTokens`, и тот перезаписал бы список токенов. Проверка: вход в 13:27:30, `expires_at` 13:32:00 (+4:30); refresh в 13:31:17, новый `expires_at` 13:35:47 (+4:30).

### 8.7. Мелочи

- ~~`StoreTokens` на refresh перезаписывает весь список токенов и теряет `token_type`, который сохранил вход.~~ Исправлено 2026-10-28: запись начинается с сохранённых токенов, `token_type` переживает refresh (проверено вживую).
- ~~`sub` ищется сначала по `ClaimTypes.NameIdentifier`. При `MapInboundClaims = false` такого claim нет, работает только вторая ветка.~~ Мёртвая ветка удалена при разбиении по ролям.
- ~~Необязательное поле ответа Keycloak (`refresh_token`, `id_token`, `token_type`) не строкового типа роняло `GetString()` мимо классификации исходов, запрос получал 500.~~ Исправлено 2026-10-28: такое поле считается отсутствующим (найдено при написании unit-тестов).
- Отсутствие записи в сторе (рестарт BFF) даёт анонимного пользователя, но мёртвая cookie остаётся в браузере до своего срока.
- Ключи Data Protection не персистятся: пересоздание контейнера делает все cookie нерасшифровываемыми.
- `TokenRefreshSemaphores` живёт в памяти процесса. При нескольких экземплярах BFF нужен распределённый замок.
