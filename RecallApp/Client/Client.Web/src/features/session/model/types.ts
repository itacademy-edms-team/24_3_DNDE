/**
 * Состояние серверной сессии BFF: только проба живости сессии (см. GET /session).
 */
export interface SessionState {
  isAuthenticated: boolean;
  /** Отображаемое имя из токена (claim name). Fallback, если доменный профиль ещё не загружен. */
  name?: string | null;
  /** Email из токена. Fallback, если доменный профиль ещё не загружен. */
  email?: string;
}
