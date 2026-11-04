/** Транспорт, с которым получена ошибка. Определяет форму тела ошибки. */
export type ApiErrorKind = "rest" | "graphql" | "realtime";

/**
 * Общий контракт ошибки любого транспорта.
 * Тело ошибки у каждого транспорта своё, общими остаются вид, повторяемость и сообщение.
 */
export interface AppError extends Error {
  readonly kind: ApiErrorKind;
  /** Имеет ли смысл повторить операцию */
  readonly retryable: boolean;
}

/** Проверяет, что значение является ошибкой нашего транспорта. */
export function isAppError(error: unknown): error is AppError {
  return error instanceof Error && "kind" in error;
}
