import type { ErrorView } from "../errorView";

/** Представление для ошибки, которую не удалось отнести к транспорту. */
export function mapUnknownError(error: unknown): ErrorView {
  return {
    message:
      error instanceof Error && error.message
        ? error.message
        : "Произошла непредвиденная ошибка. Попробуйте ещё раз.",
    fieldErrors: {},
    retryable: false,
    unauthenticated: false,
  };
}
