import { apiBaseUrl } from "./apiConfig";
import { ApiError, readProblemDetails } from "./problemDetails";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

/** Параметры запроса к API. */
export interface ApiRequestInit {
  method?: HttpMethod;
  /** Тело запроса: сериализуется в JSON. */
  body?: unknown;
  signal?: AbortSignal;
  headers?: Record<string, string>;
}

/**
 * Единая точка вызова API через BFF: cookie-сессия передаётся браузером,
 * ошибки бэкенда (RFC 7807) превращаются в ApiError.
 */
export async function apiFetch<T>(path: string, init: ApiRequestInit = {}): Promise<T> {
  const { method = "GET", body, signal, headers } = init;

  const response = await fetch(`${apiBaseUrl}${path}`, {
    method,
    // Сессия BFF живёт в cookie; без include браузер не приложит её к cross-origin запросу.
    credentials: "include",
    signal,
    headers: {
      Accept: "application/json",
      ...(body === undefined ? {} : { "Content-Type": "application/json" }),
      ...headers,
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readProblemDetails(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
