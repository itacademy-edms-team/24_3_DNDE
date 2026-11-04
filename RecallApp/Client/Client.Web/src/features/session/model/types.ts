/** Состояние серверной сессии BFF: только белый список полей (см. GET /session). */
export interface SessionState {
  isAuthenticated: boolean;
  /** Keycloak sub текущего пользователя. */
  sub?: string;
  /** Отображаемое имя из токена (claim name). */
  name?: string | null;
  email?: string;
  /** Client-роли приложения RecallApp из access-токена. */
  roles: string[];
}
