# Ручные проверки BFF: вход и обновление токенов

Документ описывает сценарии, которыми мы вручную проверяем аутентификацию BFF и обновление токенов на текущем этапе (C0). Автоматического harness ещё нет, он запланирован отдельным пунктом. Пока инструментарий такой: браузер, логи BFF, админка и события в Keycloak.

Связанные решения: ADR 0002 (BFF), 0021 (хранилище сессий), 0025 (фреймворк BFF), 0027 (политика сессий и обновление токенов).

## Что покрываем и что нет

Проверяем сейчас: базовый вход/выход, упреждающий refresh, дедупликацию параллельных обновлений, стабильность ключа сессии, независимость нескольких сессий одного пользователя, поведение при провале refresh, переживание временного сбоя Keycloak, когерентность cookie с режимом «запомнить меня».

Пока не проверяем:

- Token exchange и мостик свежего токена без потребителя (монолит), наблюдать нечего до Этапа C.
- Вытеснение уборщиком по простою установлен дефолт в 30 минут простоя и проход раз в 5 минут, за разумное время не увидеть без временного снижения порогов.

## Предусловия

1. Стек поднят: `docker compose up` из корня `RecallApp`. Контейнеры `edge-proxy`, `iam`, `iam-db`, `bff` живы.
2. Локальный Root CA импортирован в доверенные Windows (замок на `https://*.localhost` валиден).
3. В realm `recallapp` есть тестовый пользователь с клиентской ролью `app-user` (клиент `RecallApp`).
4. Хосты: приложение BFF - `https://bff.localhost`, Keycloak - `https://auth.localhost`.

### Эндпоинты BFF

| Путь | Назначение |
|---|---|
| `GET https://bff.localhost/login` | старт OIDC-входа, редирект на Keycloak, затем обратно |
| `GET https://bff.localhost/session` | состояние сессии: `{ isAuthenticated, name, roles, claims }` |
| `GET https://bff.localhost/logout` | выход: сброс cookie и OIDC sign-out |

### Где смотреть логи

- **Консоль контейнера** (`docker compose logs -f bff`) - уровень Info и выше: `Performing token refresh…`, `…succeeded`, предупреждения.
- **Файл** `Bff/Bff.Web/Host/logs/<дата>.log` - уровень Debug: `Token refresh check…`, `…skipped (double-check)…`. Эти строки в консоль не попадают.

Опорные строки лога:

| Строка | Уровень | Значит |
|---|---|---|
| `Performing token refresh for session={key}` | Info | начато реальное обновление |
| `Token refresh succeeded for session={key}` | Info | обновление прошло, тикет переписан |
| `Token refresh skipped (double-check): session={key}` | Debug | параллельный запрос не стал обновлять повторно |
| `Token refresh check: timeUntilExpiry=…, threshold=…, session={key}` | Debug | решение «рано / пора» на каждом запросе |
| `Token refresh rejected by IdP with status {code}` | Warning | Keycloak окончательно отказал (400/401), дальше sign-out |
| `Token refresh failed with transient status {code}` | Warning | временный сбой: Keycloak или прокси ответили 5xx и т.п. |
| `Token refresh request failed` | Warning | временный сбой: сеть, таймаут HttpClient (5 с) или битый JSON |
| `Token refresh deferred: IdP unavailable, session kept, retry after {time}` | Warning | сессия сохранена, повтор не раньше `{time}` |
| `Token refresh skipped (retry backoff until {time}): session={key}` | Debug | идёт пауза после сбоя, запрос не пытается обновлять |
| `BFF session id missing, falling back to sub` | Warning | cookie без идентификатора сессии (деградация на sub) |

### Ускорение timing-сценариев

Порог `RefreshThreshold` задаётся переменной окружения контейнера `bff` (в `docker-compose.override.yml`, секция `bff.environment`):

```yaml
Oidc__TokenLifetime__RefreshThreshold: "00:10:00"
```

Нужны два разных режима, не один. Порог служит сразу и триггером «пора обновлять» (`timeUntilExpiry < threshold`), и окном анти-дубля (`now - lastRefreshed < threshold`, в течение которого запрос обновление пропускает).

**Режим A — «обновлять сразу» (сценарии 2, 3, 4, 5).** Порог выше времени жизни access, напр. `00:10:00`. Тогда `timeUntilExpiry < threshold` истинно с первого запроса, refresh срабатывает немедленно. Realm трогать не нужно. Минус: после первого обновления окно анти-дубля держит пропуск 10 минут.

**Режим B — «обновлять часто» (сценарий 6).** Режим A здесь вреден: 10-минутное окно анти-дубля означает, что BFF 10 минут вообще не пытается обновлять токен, а значит не узнает про убитую в Keycloak сессию. Нужно, наоборот: короткий порог И короткий access, чтобы refresh предпринимался часто.

- env: `Oidc__TokenLifetime__RefreshThreshold: "00:00:30"`.
- realm: Access Token Lifespan = 1 минута.
- после смены войти заново (чтобы выдался короткий токен).

Так refresh предпринимается примерно раз в 30 секунд, и убитая сессия обнаруживается за ≈30–60 секунд.

После тестов вернуть: убрать env-переключатель (дефолт 1 минута) и Access Token Lifespan на боевое (5 минут, ADR 0027).

---

## Сценарий 1. Базовый вход и выход

**Цель.** Регресс после рефакторинга: вход, сессия с ролью, выход.

**Шаги.**
1. Открыть `https://bff.localhost/session` без входа.
2. Пройти `https://bff.localhost/login`, войти под тестовым пользователем.
3. Снова открыть `/session`.
4. Пройти `https://bff.localhost/logout`.
5. Снова открыть `/session`.

**Ожидаем.**
- Шаг 1: `isAuthenticated: false`.
- После входа в браузере появляется cookie `recall_bff`.
- Шаг 3: `isAuthenticated: true`, `name` заполнен, `roles` содержит `app-user`.
- Шаг 5: `isAuthenticated: false`.

---

## Сценарий 2. Упреждающее обновление токена

**Цель.** Ядро фичи: access обновляется до истечения, без разлогина.

**Предусловия.** Включён ускоряющий порог (`RefreshThreshold` = 10 минут), пользователь вошёл в аккаунт.

**Шаги.**
1. Сделать запрос `/session`.
2. Смотреть консоль `bff`.

**Ожидаем.**
- В консоли: `Performing token refresh for session=…`, затем `Token refresh succeeded for session=…`.
- `/session` по-прежнему `isAuthenticated: true`, разлогина нет.
- В событиях Keycloak (`auth.localhost` -> realm -> Sessions/Events) видно обновление токена для клиента `Bff.Web`.

---

## Сценарий 3. Дедупликация параллельных обновлений

**Цель.** На одну сессию один реальный refresh, параллельные запросы не плодят POST.

**Предусловия.** Ускоряющий порог включён, пользователь вошёл в аккаунт, cookie `recall_bff` доступна.

**Шаги.**
1. Выполнить несколько одновременных запросов `/session` одной и той же cookie. Браузером: из DevTools-консоли `Promise.all([...Array(5)].map(() => fetch('/session')))`. Либо `curl` с общим cookie-файлом, запуская запросы параллельно.
2. Смотреть консоль и файл-лог `bff`.

**Ожидаем.**
- В консоли ровно одна пара `Performing… / …succeeded`.
- В файл-логе остальные запросы: `…skipped (double-check): session=…`.
- В Keycloak один POST обновления на серию, не несколько.

---

## Сценарий 4. Стабильность идентификатора сессии через refresh

**Цель.** `bff_session_id` не меняется при обновлении токенов.

**Шаги.**
1. Войти, вызвать `/session` до порога - зафиксировать `session=…` в логе.
2. Дождаться (или форсировать порогом) обновления.
3. Сравнить `session=…` в строках `Performing…` / `…succeeded` с зафиксированным.

**Ожидаем.** Значение `session=` одно и то же до и после обновления.

---

## Сценарий 5. Несколько сессий одного пользователя

**Цель.** Вход с двух устройств/браузеров не связывает сессии: каждая цепочка рефрешится независимо, обновление одной не мешает обновлению другой.

**Предусловия.** Ускоряющий порог включён.

**Шаги.**
1. Войти под одним аккаунтом в браузере A и браузере B (либо два независимых cookie-jar).
2. Подёргать `/session` в обоих.
3. Смотреть логи.

**Ожидаем.**
- Два разных значения `session=` (по одному на браузер).
- Обновление идёт отдельно для каждого: активность в A не отменяет обновление в B (нет ситуации, когда B перестаёт обновляться, пока активен A).

---

## Сценарий 6. Провал refresh - выход и повторная аутентификация

**Цель.** Протухший или отозванный refresh ведёт к sign-out и возврату на вход (при живой SSO-сессии Keycloak - без пароля).

**Предусловия.** RefreshThreshold=00:00:30; AccesTokenLifespan=00:01:00 (в Keycloak).

**Предусловия.** Режим B (короткий порог `00:00:30` + Access Token Lifespan 1 минута), вход в аккаунт выполнен заново после смены паметров.

**Шаги.**
1. Войти в BFF.
2. В админке Keycloak завершить эту сессию пользователя (Sessions -> Logout).
3. Дёргать `/session` раз в ~15 секунд.

**Ожидаем.**
- Через ≈30–60 секунд в консоли: `Token refresh rejected by IdP with status BadRequest`, затем BFF делает sign-out. Строки `deferred` здесь быть не должно: отказ 400 не временный сбой.
- Cookie `recall_bff` сброшена, `/session` -> `isAuthenticated: false`.
- Переход на `/login`: если SSO-сессия Keycloak ещё жива - вход без ввода пароля (silent re-auth), иначе полный вход.

**Важно про тайминг ответа.** Sign-out в middleware вешает очистку cookie на ответ, но принципал текущего запроса уже проставлен на `UseAuthentication`. Поэтому запрос, на котором refresh провалился, может ещё раз отдать `isAuthenticated: true` (с заголовком удаления cookie), а `false` придёт на следующем поллинге, когда cookie уже нет. Это отставание на один запрос, не ошибка.

**Про задержку обнаружения.** Убийство сессии в Keycloak не разлогинивает BFF мгновенно. BFF узнаёт об этом только на следующей неудачной попытке refresh, поэтому окно обнаружения ограничено частотой refresh, то есть сроком жизни access-токена (ADR 0027).

---

## Сценарий 7. Cookie следует режиму «запомнить меня»

> Статус: **не прогнан**. Код входа менялся 2026-10-27 (события OIDC объединены в `OnTokenValidated`, окно cookie через `SessionCookieLifetime.ResetWindow`).

**Цель.** Срок и персистентность cookie `recall_bff` совпадают с режимом входа (ADR 0027): без галочки cookie сессионная с окном SSO Idle (30 минут), с галочкой persistent с окном SSO Idle Remember Me (30 дней).

**Предусловия.** В realm включён Remember Me, SSO Session Idle = 30 минут, SSO Session Idle Remember Me = 30 дней. Порог `RefreshThreshold` по умолчанию, Access Token Lifespan боевой (5 минут).

**Где смотреть.** DevTools → Application → Cookies → `https://bff.localhost` → `recall_bff`, столбец Expires / Max-Age: `Session` или дата. Либо Network → ответ на `/signin-oidc` → заголовок `Set-Cookie`, есть ли в нём `expires=`.

**Шаги A: без галочки.**
1. `/logout`, затем `/login`, войти без отметки Remember me.
2. Посмотреть cookie.
3. Полностью закрыть браузер (все окна), открыть `/session`.

**Шаги B: с галочкой.**
1. `/logout`, затем `/login`, войти с отметкой Remember me.
2. Посмотреть cookie.
3. Полностью закрыть браузер, открыть `/session`.

**Шаг C: скольжение на refresh.** В сессии из B дождаться refresh (или включить режим A ускорения) и сравнить `expires=` в `Set-Cookie` ответа, на котором прошёл refresh, с предыдущим.

**Ожидаем.**
- A: Expires = `Session`; после перезапуска браузера `/session` → `isAuthenticated: false`.
- B: Expires ≈ сейчас + 30 дней; после перезапуска браузера `/session` → `isAuthenticated: true` (протухший access обновится refresh-ем).
- C: `expires=` сдвинулся вперёд на время, прошедшее с прошлой выдачи cookie.

**Если в B получилось `Session`.** Не сработала эвристика: проверить, что в realm SSO Session Idle Remember Me действительно 30 дней, и что `refresh_expires_in` при входе с галочкой больше `Oidc:TokenLifetime:RememberMeThreshold` (по умолчанию 1 день).

---

## Сценарий 8. Keycloak временно недоступен: сессия выживает

> Статус: **не прогнан**. Код добавлен 2026-10-27: исходы refresh `Success` / `Rejected` / `Unavailable`, пауза `Oidc:TokenLifetime:RefreshRetryBackoff`.

**Цель.** Временный сбой Keycloak (завис, остановлен, отвечает 5xx) не разлогинивает пользователя. BFF сохраняет сессию и повторяет refresh не чаще раза в `RefreshRetryBackoff` (15 секунд). Когда Keycloak вернётся, токены обновятся как обычно.

**Предусловия.** Режим B (порог `00:00:30` + Access Token Lifespan 1 минута), вход выполнен заново. Попытки refresh тогда идут примерно раз в 30 секунд, и сбой проявляется быстро.

**Как BFF ходит в Keycloak.** `Authority` указывает на `https://ah.localhost`, запрос проходит через `edge-proxy` (nginx) в контейнер `iam`. Два способа уронить Keycloak ведут в разные ветки кода:

| Вариант | Команда | Что видит BFF | Строка лога |
|---|---|---|---|
| A: заморозить | `docker compose pause iam` | соединение есть, ответа нет, срабатывает таймаут HttpClient 5 с | `Token refresh request failed` (`TaskCanceledException`) |
| B: остановить | `docker compose stop iam` | nginx отвечает 502 или соединение не устанавливается | `Token refresh failed with transient status BadGateway` либо `Token refresh request failed` |

Основной вариант A. В B есть ловушка инфраструктуры: nginx сейчас резолвит `iam` один раз при старте, и после `start iam` контейнер может получить другой IP. Если после возврата Keycloak BFF всё ещё видит 502, перезапустить прокси: `docker compose restart edge-proxy`.

**Шаги A: заморозка.**
1. Войти, `/session` → `isAuthenticated: true`.
2. `docker compose pause iam`.
3. Минуту запрашивать `/session` раз в ~5 секунд.
4. `docker compose unpause iam` (Keycloak оживает сразу), ещё минуту запрашивать `/session` раз в ~5 секунд.

**Шаги B: остановка.** Как в A, только на шаге 2 `docker compose stop iam`, а на шаге 4 `docker compose start iam` и ожидание готовности Keycloak (в `docker compose logs iam` строка `Listening on`).

**Ожидаем (оба варианта).**
- Во время сбоя `/session` всё время отвечает `isAuthenticated: true`, cookie `recall_bff` не сброшена.
- На каждую попытку в консоли два Warning: причина (см. таблицу выше) и `Token refresh deferred: IdP unavailable, session kept, retry after …`.
- Попытки идут не чаще раза в 15 секунд. Между ними в файл-логе Debug `Token refresh skipped (retry backoff until …)`.
- В варианте A запрос с попыткой отвечает примерно через 5 секунд, остальные запросы во время паузы отвечают сразу.
- После возврата Keycloak ближайшая попытка даёт `Performing token refresh…`, затем `Token refresh succeeded…`.

**Известное ограничение.** Если сбой длится дольше остатка жизни access-токена, access протухает, а BFF-сессия остаётся живой. `/session` этого не покажет, он отвечает из cookie. Вызовы API с таким токеном получали бы 401 до возврата Keycloak, но потребителя access пока нет (до Этапа C).

**Пограничный случай варианта A.** Замороженный Keycloak мог успеть принять запрос в буфер сокета. После `unpause` он его обработает и выпустит новый refresh-токен, хотя BFF уже отбросил ответ по таймауту и держит старый. При включённом Revoke Refresh Token повтор со старым токеном получит 400: `Token refresh rejected by IdP with status BadRequest`, затем sign-out. Такой исход подтверждает известный риск короткого таймаута и регрессом сценария не считается. Записать в итог прогона, решать отдельно.

---

## Переключатели realm для удобства тестов

Эти изменения в realm `recallapp` ускоряют наблюдение. Применяются вручную, после теста откатываются.

| Параметр | Для чего | Значение теста |
|---|---|---|
| Access Token Lifespan | ускорить подход к порогу без env-переключателя | 1–2 минуты |
| SSO Session Idle | быстрее проверить смерть SSO-сессии (сценарий 6) | короткое, напр. 2 минуты |
| Revoke Refresh Token | проверить, что переигровка старого refresh рвёт цепочку | включено (по 0027) |
