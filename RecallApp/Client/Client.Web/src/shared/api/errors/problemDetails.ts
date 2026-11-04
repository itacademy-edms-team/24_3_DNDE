/** Формат тела ошибки RFC 9457 ProblemDetails, как его отдаёт бэкенд. */
export interface ProblemDetails {
  type?: string;
  /** Машиночитаемый код ошибки в PascalCase (например, InvalidEmail, NotAuthenticated). */
  title?: string;
  status?: number;
  /** Пояснение для разработчика; не всегда пригодно для показа пользователю. */
  detail?: string;
  instance?: string;
  /** Ошибки валидации: ключ - имя поля (target), значение - список сообщений. */
  errors?: Record<string, string[]>;
  /** Кто отказал: BFF (сессия, CSRF) или домен (App). Коды у обоих в PascalCase, различает их это поле. */
  source?: "bff" | "app";
  [extension: string]: unknown;
}
