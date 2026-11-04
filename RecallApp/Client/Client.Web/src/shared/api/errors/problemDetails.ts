/** Формат тела ошибки RFC 7807 ProblemDetails, как его отдаёт бэкенд. */
export interface ProblemDetails {
  type?: string;
  /** Машиночитаемый код ошибки в PascalCase или snake_case-коде BFF (например, not_authenticated). */
  title?: string;
  status?: number;
  /** Пояснение для разработчика; не всегда пригодно для показа пользователю. */
  detail?: string;
  instance?: string;
  /** Ошибки валидации: ключ - имя поля (target), значение - список сообщений. */
  errors?: Record<string, string[]>;
  /** Кто отказал: BFF (сессия, CSRF) или домен (App). */
  source?: "bff" | "app";
  [extension: string]: unknown;
}
