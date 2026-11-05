import { describe, expect, it } from "vitest";

import { restCodeMessage, restCodeMessages } from "./restCodes";

describe("restCodeMessages", () => {
  // Коды из BFF и монолита должны быть в одном стиле
  it("uses the shared PascalCase code format for every key", () => {
    const invalid = Object.keys(restCodeMessages).filter(
      (code) => !/^[A-Z][A-Za-z0-9]*$/.test(code),
    );

    expect(invalid).toEqual([]);
  });

  it("returns the message for a known code and null for an unknown one", () => {
    expect(restCodeMessage("SessionExpired")).toBe("Сессия истекла. Войдите заново.");
    expect(restCodeMessage("session_expired")).toBeNull();
    expect(restCodeMessage(undefined)).toBeNull();
  });
});
