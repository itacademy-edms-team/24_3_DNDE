# Identity and Access Management - Keycloak

## Первичная настройка Realm

1. Создайте Realm `recallapp`. Перейдите в него.

2. Создайте клиент `RecallApp`:
   1. Client type: `OpenID Connect`
   2. Client ID: `RecallApp`
   3. Client authentication: ON
   4. Authorization: OFF
   5. Authentication flow: Standard flow ON
   6. Require PKCE: ON
   7. PKCE Method: S256
   8. Остальное: OFF
   9. Valid redirect URIs: `https://bff.localhost/signin-oidc`
   10. Valid post logout redirect URIs: `https://bff.localhost/signout-callback-oidc`
   11. Web origins: `https://bff.localhost`

3. Перейдите в созданный ранее клиент. Запомните Client Secret - он поднадобится для Bff.Web

4. Снова перейдите в клиент, вкладка Roles -> Create Role
   1. Role Name: `app-user`
   2. Description: `Пользователь приложения RecallApp`

5. Перейдите в Realm Settings -> User Registration -> Assign role -> Client roles
   1. Найдите в списке app-user, отметьте
   2. Assign

6. Перейдите на вкладку Users -> Create User
   1. Username: `testuser`
   2. Email: `test@recallapp.local`
   3. First Name: `test`
   4. Last Name: `user`

7. Откройте созданного пользователя -> Credentials -> Set password
   1. Задайте пароль, запомните его
   3. Temporary: OFF
