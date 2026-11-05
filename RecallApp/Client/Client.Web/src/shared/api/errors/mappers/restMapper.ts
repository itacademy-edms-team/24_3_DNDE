import { restCodeMessage } from "@/shared/api/errors/restCodes";
import type { ErrorView } from "@/shared/api/errors/errorView";
import type { RestApiError } from "@/shared/api/errors/restApiError";

/** Переводит ошибку REST-вызова в представление для интерфейса. */
export function mapRestError(error: RestApiError): ErrorView {
  const problem = error.problem;

  if (error.status === null) {
    return {
      message: "Нет соединения с сервером. Проверьте сеть и попробуйте ещё раз.",
      fieldErrors: {},
      retryable: true,
      unauthenticated: false,
    };
  }

  if (error.status === 401) {
    return {
      message: restCodeMessage(problem?.title) ?? "Требуется вход в аккаунт.",
      fieldErrors: {},
      retryable: false,
      unauthenticated: true,
    };
  }

  if (error.status === 403) {
    return {
      message: restCodeMessage(problem?.title) ?? "Действие недоступно для вашей учётной записи.",
      fieldErrors: {},
      retryable: false,
      unauthenticated: false,
    };
  }

  if (error.status === 400 && problem?.errors) {
    return {
      message: problem.detail ?? restCodeMessage(problem.title) ?? "Проверьте заполнение полей.",
      fieldErrors: problem.errors,
      retryable: false,
      unauthenticated: false,
    };
  }

  if (error.status >= 500) {
    return {
      message: "Сервис временно недоступен. Попробуйте позже.",
      fieldErrors: {},
      retryable: true,
      unauthenticated: false,
    };
  }

  return {
    message:
      restCodeMessage(problem?.title) ??
      problem?.detail ??
      "Не удалось выполнить запрос. Попробуйте ещё раз.",
    fieldErrors: {},
    retryable: error.retryable,
    unauthenticated: false,
  };
}
