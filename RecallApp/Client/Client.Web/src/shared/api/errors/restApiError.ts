import type { AppError } from "./appError";
import type { ProblemDetails } from "./problemDetails";

/** Ошибка REST-вызова: HTTP-статус и разобранный ProblemDetails, если он был в ответе. */
export class RestApiError extends Error implements AppError {
  readonly kind = "rest" as const;
  /** HTTP-статус; null - ответа не было (сеть, таймаут, отмена). */
  readonly status: number | null;
  readonly problem: ProblemDetails | null;
  readonly retryable: boolean;

  constructor(status: number | null, problem: ProblemDetails | null, message?: string) {
    super(message ?? problem?.title ?? `HTTP ${status ?? "network"}`);
    this.name = "RestApiError";
    this.status = status;
    this.problem = problem;
    this.retryable = status === null || status >= 500 || status === 429;
  }

  /** 401 - сессии нет или она истекла: пользователю нужно войти заново. */
  get isUnauthenticated(): boolean {
    return this.status === 401;
  }
}

/** Проверяет, что тело ответа похоже на ProblemDetails. */
export function isProblemDetails(data: unknown): data is ProblemDetails {
  return typeof data === "object" && data !== null && ("title" in data || "status" in data);
}
