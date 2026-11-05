import { isAppError } from "./appError";
import { mapRestError } from "./mappers/restMapper";
import { mapUnknownError } from "./mappers/unknownError";
import type { RestApiError } from "./restApiError";

/** Представление ошибки для интерфейса: без транспорта, статусов и stack trace. */
export interface ErrorView {
  /** Текст для пользователя. */
  message: string;
  /** Ошибки по полям формы: ключ - имя поля, значение - сообщения. */
  fieldErrors: Record<string, string[]>;
  /** Есть ли смысл предложить повтор. */
  retryable: boolean;
  /** Нужно увести пользователя на вход. */
  unauthenticated: boolean;
}

/**
 * Единая точка перевода ошибки в представление для интерфейса.
 * Новый транспорт добавляет сюда свою ветку, потребители не меняются.
 */
export function toErrorView(error: unknown): ErrorView {
  if (!isAppError(error)) {
    return mapUnknownError(error);
  }

  switch (error.kind) {
    case "rest":
      return mapRestError(error as RestApiError);
    default:
      // graphql и realtime появятся вместе со своими мапперами.
      return mapUnknownError(error);
  }
}
