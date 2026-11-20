import { describe, expect, it } from "vitest";

import { formatUpdatedAt } from "./formatUpdatedAt";

describe("formatUpdatedAt", () => {
  const now = new Date("2026-11-14T12:00:00Z").getTime();

  it("returns «только что» for less than a minute in the past", () => {
    expect(formatUpdatedAt("2026-11-14T11:59:30Z", now)).toBe("только что");
  });

  it("formats minutes in the past", () => {
    expect(formatUpdatedAt("2026-11-14T11:55:00Z", now)).toBe("5 минут назад");
  });

  it("formats days in the past", () => {
    expect(formatUpdatedAt("2026-11-11T12:00:00Z", now)).toBe("3 дня назад");
  });
});
