import { authClient } from "../../../shared/api/clients/authClient";
import type { SessionState } from "../model/types";

/** Читает состояние сессии у BFF. Эндпоинт не кэшируется на сервере. */
export async function getSession(signal?: AbortSignal): Promise<SessionState> {
  const { data } = await authClient.get<SessionState>("/session", { signal });
  return data;
}
