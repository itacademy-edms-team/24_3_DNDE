import { bffBaseUrl } from "../../../shared/api/apiConfig";

/**
 * Абсолютный URL входа на BFF. Вход - переход браузера целиком (страницы входа отдаёт Keycloak),
 * а не запрос из скрипта: так работает редирект OIDC и не вмешивается CORS.
 */
export function loginUrl(returnTo?: string): string {
  const target = returnTo ?? `${window.location.pathname}${window.location.search}`;
  return `${bffBaseUrl}/login?returnUrl=${encodeURIComponent(target)}`;
}

/** Абсолютный URL выхода: используется только как action HTML-формы (POST). */
export const logoutUrl: string = `${bffBaseUrl}/logout`;
