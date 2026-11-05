import { bffBaseUrl } from "@/shared/api/apiConfig";
import { createRestClient } from "./createRestClient";

/**
 * Клиент служебных эндпоинтов BFF (GET /session).
 * Вход и выход через него не ходят: это навигация и HTML-форма, CORS на них не действует.
 */
export const authClient = createRestClient(bffBaseUrl);
