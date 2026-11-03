import { bffBaseUrl } from "../../../shared/api/apiConfig";
import { ApiError, readProblemDetails } from "../../../shared/api/problemDetails";

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
  const response = await fetch(`${bffBaseUrl}/session`, {
    credentials: "include",
    signal,
    headers: { Accept: "application/json" },
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readProblemDetails(response));
  }

  return (await response.json()) as SessionState;
}
