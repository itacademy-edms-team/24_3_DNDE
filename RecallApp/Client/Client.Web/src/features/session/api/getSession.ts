import { authClient } from "../../../shared/api/clients/authClient";

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

/** Читает состояние сессии у BFF. Эндпоинт не кэшируется на сервере. */
export async function getSession(signal?: AbortSignal): Promise<SessionState> {
  const { data } = await authClient.get<SessionState>("/session", { signal });
  return data;
}
