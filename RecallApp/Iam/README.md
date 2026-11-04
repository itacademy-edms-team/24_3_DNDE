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
