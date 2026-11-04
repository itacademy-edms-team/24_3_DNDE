import axios, { type AxiosInstance } from "axios";

import { isProblemDetails, RestApiError } from "../errors/restApiError";

/** Переводит любую ошибку axios в RestApiError: тело бэкенда сохраняется, обёртка отбрасывается. */
export function normalizeRestError(error: unknown): RestApiError {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status ?? null;
    const data = error.response?.data;
    const problem = isProblemDetails(data) ? data : null;
    return new RestApiError(status, problem, problem?.title ?? error.message);
  }

  return new RestApiError(
    null,
    null,
    error instanceof Error ? error.message : "Неизвестная ошибка транспорта",
  );
}

/**
 * Создаёт REST-клиент для указанного адреса.
 * Cookie-сессия BFF передаётся браузером (withCredentials), ошибки нормализуются интерцептором.
 */
export function createRestClient(baseURL: string): AxiosInstance {
  const client = axios.create({
    baseURL,
    withCredentials: true,
    // fetch-адаптер вместо XHR: тот же браузерный транспорт, что был до axios,
    // и запросы можно подменять в тестах через stubGlobal("fetch", ...).
    adapter: "fetch",
    headers: { Accept: "application/json" },
  });

  client.interceptors.response.use(
    (response) => response,
    (error: unknown) => Promise.reject(normalizeRestError(error)),
  );

  return client;
}
