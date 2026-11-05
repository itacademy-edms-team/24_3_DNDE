/// <reference types="vite/client" />

// Явная типизация переменных окружения, которые начинаются с VITE_ и нужны в коде клиента.
interface ImportMetaEnv {
  /** Базовый URL API через BFF (YARP -> монолит), например https://bff.localhost/api */
  readonly VITE_API_BASE_URL?: string;
  /** Базовый URL BFF для служебных эндпоинтов сессии (/login, /logout, /session) */
  readonly VITE_BFF_BASE_URL?: string;
  // Сюда добавляем любые другие переменные окружения, которые начинаются с VITE_
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
