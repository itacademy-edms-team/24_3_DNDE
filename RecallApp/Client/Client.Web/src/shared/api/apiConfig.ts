/**
 * Адреса бэкенда: API (через BFF, YARP -> монолит) и служебные эндпоинты BFF.
 * Значения приходят из окружения сборки, в коде не хардкодятся.
 */
export const apiBaseUrl: string = import.meta.env.VITE_API_BASE_URL ?? "https://bff.recallapp.localhost/api";

/** Корень BFF: /login, /logout, /session не входят в проксируемый префикс /api. */
export const bffBaseUrl: string = import.meta.env.VITE_BFF_BASE_URL ?? "https://bff.recallapp.localhost";
