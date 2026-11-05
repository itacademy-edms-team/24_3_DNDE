import { bffBaseUrl } from "@/shared/api/apiConfig";

/**
 * Абсолютный URL входа на BFF. Вход - переход браузера целиком (страницы входа отдаёт Keycloak),
 * а не запрос из скрипта: так работает редирект OIDC и не вмешивается CORS.
 * Путь возврата передаёт вызывающая сторона (её источник - useLocation/состояние роутера).
 */
export function loginUrl(returnTo: string): string {
  return `${bffBaseUrl}/login?returnUrl=${encodeURIComponent(returnTo)}`;
}

/** Абсолютный URL выхода: используется только как action HTML-формы (POST). */
export const logoutUrl: string = `${bffBaseUrl}/logout`;
