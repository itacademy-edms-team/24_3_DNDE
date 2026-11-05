import { describe, expect, it } from "vitest";

import { toErrorView } from "./errorView";
import { RestApiError } from "./restApiError";

describe("toErrorView", () => {
  it("shows a network message when the request got no response", () => {
    const view = toErrorView(new RestApiError(null, null, "Network Error"));

    expect(view.retryable).toBe(true);
    expect(view.unauthenticated).toBe(false);
    expect(view.message).toContain("Нет соединения");
  });

  it("marks 401 as unauthenticated and uses the code dictionary", () => {
    const view = toErrorView(new RestApiError(401, { title: "SessionExpired", status: 401 }));

    expect(view.unauthenticated).toBe(true);
    expect(view.retryable).toBe(false);
    expect(view.message).toBe("Сессия истекла. Войдите заново.");
  });

  it("returns field errors for a validation problem", () => {
    const view = toErrorView(
      new RestApiError(400, {
        title: "InvalidDisplayName",
        detail: "Проверьте имя",
        errors: { "user.displayName": ["Слишком длинное"] },
      }),
    );

    expect(view.fieldErrors).toEqual({ "user.displayName": ["Слишком длинное"] });
    expect(view.message).toBe("Проверьте имя");
  });

  it("falls back to detail for an unknown code", () => {
    const view = toErrorView(
      new RestApiError(409, { title: "SomeUnknownCode", detail: "Конфликт версий" }),
    );

    expect(view.message).toBe("Конфликт версий");
  });

  it("shows a generic message when nothing is known", () => {
    const view = toErrorView(new RestApiError(404, null));

    expect(view.message).toContain("Не удалось выполнить запрос");
  });

  it("shows a service message for 5xx", () => {
    const view = toErrorView(new RestApiError(500, { title: "Internal" }));

    expect(view.retryable).toBe(true);
    expect(view.message).toContain("временно недоступен");
  });

  it("maps a plain Error to its message", () => {
    expect(toErrorView(new Error("Что-то сломалось")).message).toBe("Что-то сломалось");
  });

  it("maps an unknown value to a generic message", () => {
    expect(toErrorView("строковая ошибка").message).toContain("непредвиденная");
  });
});
