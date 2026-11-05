/** Профиль пользователя в домене Accounts (контракт GET /api/users/me). */
export interface UserProfile {
  id: string;
  displayName: string;
  email: string;
  /**
   * Признак подтверждения email. Домен его не хранит: значение вычисляется монолитом
   * из claim email_verified токена Keycloak (в том числе для входа через внешних провайдеров),
   * поэтому поле приходит в ответе каждый раз и не кэшируется как данные аккаунта.
   */
  emailVerified: boolean;
}
