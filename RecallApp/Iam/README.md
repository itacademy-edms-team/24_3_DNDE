# Identity and Access Management - Keycloak

## Первичная настройка Realm

### RecallApp

1. Создайте Realm `recallapp`. Перейдите в него.

2. Создайте клиент `RecallApp`:
   1. Client type: `OpenID Connect`
   2. Client ID: `RecallApp`
   3. Остальное: OFF

3. Перейдите в клиент, вкладка Roles -> Create Role
   1. Role Name: `app-user`
   2. Description: `Пользователь приложения RecallApp`

4. Перейдите в Realm Settings -> User Registration -> Assign role -> Client roles
   1. Найдите в списке app-user, отметьте
   2. Assign

5. Перейдите на вкладку Users -> Create User
   1. Username: `testuser`
   2. Email: `test@recallapp.local`
   3. First Name: `test`
   4. Last Name: `user`

6. Откройте созданного пользователя -> Credentials -> Set password
   1. Задайте пароль, запомните его
   2. Temporary: OFF

### Bff.Web

1. Создайте клиент `Bff.Web`:
   1. Client type: `OpenID Connect`
   2. Client ID: `Bff.Web`
   3. Client authentication: ON
   4. Authorization: OFF
   5. Authentication flow: Standard flow ON
   6. Require PKCE: ON
   7. PKCE Method: S256
   8. Остальное: OFF
   9. Valid redirect URIs: `https://bff.recallapp.localhost/signin-oidc`
   10. Valid post logout redirect URIs: `https://bff.recallapp.localhost/signout-callback-oidc`
   11. Web origins: `https://bff.recallapp.localhost`

2. Перейдите в созданный ранее клиент. Запомните Client Secret - он поднадобится для Bff.Web

3. В корневом .env добавьте/обновите строчку:
```
BFF_WEB_OIDC_CLIENT_SECRET=<ВАШ_CLIENT_SECRET_ИЗ_BFF_WEB>
```

> Клиент создаётся с включённым `Full scope allowed`. Выключать его нужно после настройки ролей - см. раздел [Роли и audience в токене](#роли-и-audience-в-токене).

## Роли и audience в токене

В Keycloak 26 настройки клиента живут на вкладке `Clients -> Bff.Web -> Client details -> Bff.Web-dedicated -> Scope`: там переключатель `Full scope allowed` и таблица назначенных ролей (`Assign role`).

Зачем менять дефолт: встроенный client scope `roles` содержит мапперы `realm roles` и `client roles` с **незаданным** `Client ID`. Такой маппер складывает в токен роли всех клиентов, где у пользователя есть роли (`resource_access.<client>.roles`), а `audience resolve` добавляет эти клиенты в `aud`. Состав нашего токена начинает зависеть от чужих клиентов в realm - это лишний контракт и лишние права. Вместо этого:

1. Client scopes -> Create client scope:
   1. Name: `recallapp-roles`
   2. Type: `None`
   3. Protocol: `openid-connect`
   4. Display on consent screen: OFF
   5. Include in token scope: ON

2. Внутри скоупа: Mappers -> Add mapper -> By configuration -> `User Client Role` (`roles` из списка predefined не подходит - у него нет Client ID, и он пишет роли вложенно):
   1. Name: `RecallApp client roles`
   2. Client ID: `RecallApp` (клиент-роль-холдер, не `Bff.Web`)
   3. Multivalued: ON
   4. Token Claim Name: `roles`
   5. Claim JSON Type: `String`
   6. Client Role prefix: `RecallApp:` - префикс равен Client ID: за счёт этого одинаковые роли разных клиентов не сливаются в одном плоском claim'е
   7. Add to access token: ON
   8. Add to ID token: ON
   9. Add to lightweight access token / userinfo / introspection: OFF

3. Add mapper -> By configuration -> `Audience` (не `Audience Resolve`: тот вычисляет audience из разрешённых ролей и растёт вместе с realm):
   1. Name: `RecallApp audience`
   2. Included Client Audience: `RecallApp`
   3. Add to access token: ON
   4. Add to ID token: OFF (у ID-токена audience - `Bff.Web`, его проверяет OIDC-хендлер)

4. Clients -> Bff.Web -> Client scopes -> Add client scope -> `recallapp-roles` -> Add -> `Default`.

5. Clients -> Bff.Web -> Client details -> Bff.Web-dedicated -> Scope -> Assign role -> Filter by clients -> `RecallApp` -> `app-user` -> Assign.

6. Там же: `Full scope allowed` -> OFF.

7. Clients -> Bff.Web -> Client scopes -> снять встроенный `roles` (Remove). Иначе он продолжит добавлять `resource_access` в access-токен.

### Проверка

1. Перелогиньтесь: refresh перевыпускает токены, но cookie-тикет BFF хранит снимок claims на момент входа, поэтому `IsInRole` оживёт только после нового логина.
2. `https://bff.recallapp.localhost/debug/session` -> `roles: ["RecallApp:app-user"]`.
3. В access-токене: `roles: ["RecallApp:app-user"]`, `aud: "RecallApp"`, нет `resource_access` и `realm_access`.
4. В ID-токене: `roles: ["RecallApp:app-user"]`, `aud: "Bff.Web"`.

### Правило на будущее

Новый роль-холдер (например, back-office) - отдельный client scope с мапперами `User Client Role` (свой `Client ID`, `Client Role prefix` равен этому `Client ID`) и `Audience`, плюс своё `Assign role` у того клиента, который выпускает токены. Роли в `roles` всегда пространственно именованы (`RecallApp:app-user`, `RecallAppBackOffice:bo-admin`), благодаря чему один BFF может безопасно обслуживать несколько роль-холдеров.

Если какому-то потребителю понадобится каноничный для Keycloak вложенный формат `resource_access.<client>.roles`, в тот же скоуп добавляется второй маппер с `Token Claim Name = resource_access.<client>.roles`.

### Правило на будущее

Новый роль-холдер (например, back-office) - отдельный client scope с мапперами `User Client Role` (свой `Client ID`) и `Audience`, плюс своё `Assign role` у того клиента, который выпускает токены. Не смешивать мапперы разных клиентов в одном скоупе: роли сольются в один claim. Роль, заведённая в чужом клиенте, даст права в нашем, что не правильно.

## Политика сессий (ADR 0027)

1. Перейдите в realm `recallapp`.

2. Realm Settings -> Login:
- Remember me: `ON`

3. Realm Settings -> Sessions:
- SSO Session Idle: `30 Minutes`
- SSO Session Max: `10 Hours`
- SSO Session Idle Remember Me: `30 Days`
- SSO Session Max Remember Me: `90 Days`

4. Realm Settings -> Tokens:
- Access Token Lifespan: `5 Minutes`
- Revoke Refresh Token: `ON`
- Refresh Token Max Reuse: `0`

## Тестовый realm `recallapp-test`

Нужен интеграционным тестам BFF категории `Keycloak` (`Bff/Bff.Web/tests`). Тесты получают токены через Direct Access Grants, минуя страницы входа, и затем обновляют их как BFF. Keycloak обновляет refresh-токен только тому клиенту, которому его выдал. Поэтому DAG включается на отдельном клиенте в отдельном realm, а основной `Bff.Web` и realm `recallapp` не меняются.

Realm в git не хранится, настраивается вручную по шагам ниже.

> Роли и audience для тестового realm не настраиваются: тесты категории `Keycloak` проверяют выдачу и обновление токенов через Direct Access Grants, роли им не нужны. Если понадобятся - повторите раздел [Роли и audience в токене](#роли-и-audience-в-токене) для клиента `Bff.Web.Test`.

### Realm и роли

1. Создайте Realm `recallapp-test`. Перейдите в него.

2. Создайте клиент `RecallApp` и роль `app-user` так же, как в разделе [RecallApp](#recallapp) (шаги 2–4): тип `OpenID Connect`, всё остальное OFF, роль назначена по умолчанию через User Registration.

### Клиент Bff.Web.Test

1. Создайте клиент `Bff.Web.Test`:
   1. Client type: `OpenID Connect`
   2. Client ID: `Bff.Web.Test`
   3. Client authentication: ON
   4. Authorization: OFF
   5. Authentication flow: Standard flow OFF, Direct access grants ON
   6. Остальное: OFF
   7. Redirect URI и Web origins не нужны

2. Откройте вкладку Credentials клиента и скопируйте Client Secret.

### Тестовый пользователь

Пользователя создавайте после того, как роль `app-user` назначена по умолчанию: роли по умолчанию выдаются только новым пользователям.

1. Users -> Create User:
   1. Username: `testuser`
   2. Email: `test@recallapp.local`, Email verified: ON
   3. First Name: `test`
   4. Last Name: `user`

2. Credentials -> Set password, Temporary: OFF.

3. Убедитесь, что у пользователя нет Required user actions. DAG отказывает во входе, пока у пользователя есть незавершённые действия (например, Verify Profile при незаполненном профиле).

### Политика сессий

Те же значения, что в разделе [Политика сессий](#политика-сессий-adr-0027), кроме Remember me: тестам он не нужен, потому что DAG не создаёт сессию Remember Me.

- Sessions: SSO Session Idle `30 Minutes`, SSO Session Max `10 Hours`
- Tokens: Access Token Lifespan `5 Minutes`, Revoke Refresh Token `ON`, Refresh Token Max Reuse `0`

### Секреты

В корневом `.env` добавьте:
```
BFF_TEST_OIDC_CLIENT_SECRET=<CLIENT_SECRET_ИЗ_BFF_WEB_TEST>
BFF_TEST_USER_PASSWORD=<ПАРОЛЬ_TESTUSER_ИЗ_RECALLAPP_TEST>
```

## Бэкап realm

Ручной экспорт realm в папку вне репозитория (с пользователями):

```powershell
./Iam/export-realm.ps1 -OutDir D:\backups\recallapp
```

Тестовый realm выгружается тем же скриптом с параметром `-Realm`:

```powershell
./Iam/export-realm.ps1 -OutDir D:\backups\recallapp -Realm recallapp-test
```

## Настройка Яндекса как IdP в Keycloak

1. Создать клиент в Яндекс: https://oauth.yandex.ru/client/new/auth

RedirectUri: `https://auth.localhost/realms/recallapp/broker/yandex/endpoint`
Разрешения ставить минимальные: почта, имя пользователя.

1. Запомнить ClientId и ClientSecret.
2. Перейти в админку Keycloak -> realm recallapp -> IdentityProviders -> OAuthV2
3. Authorization URL: `https://oauth.yandex.ru/authorize`
4. Token URL: `https://oauth.yandex.ru/token`
5. User Info URL: `https://login.yandex.ru/info?format=json`
6. Client Authentication: `Client secret sent in the request body`
7. Client Id: из созданного клиента
8. Client Secret: из созданного клиента
9. ID Claim: `id`
10. Username Claim: `login`
11. Email Claim: `default_email`
12. Name Claim: `display_name`
13. Given name Claim: `first_name`
14. Family name Claim: `last_name`
15. Add (создаём IdP)
16. Trust Email: `True`
17. Forward Parameters: `Prompt` -> `Select Account`
18. Save (сохранить IdP)

Теперь при входе через Keycloak можно будет войти через Яндекс.

**Поведение Keycloak на вход по почте с уже существующим аккаунтом:**
Keycloak показывает окна, что пользователь уже есть в системе и предлагает ему объединить и актуализировать данные. Для подтверждения слияния вводится пароль. Работает, если не включён `Duplicate Email`.
