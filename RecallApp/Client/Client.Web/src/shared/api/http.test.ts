import { afterEach, describe, expect, it, vi } from "vitest";

import { apiFetch } from "./http";

/** Ответ с проблемой в формате RFC 7807. */
function problemResponse(status: number, problem: Record<string, unknown>): Response {
  return new Response(JSON.stringify(problem), {
    status,
    headers: { "Content-Type": "application/problem+json" },
  });
}

describe("apiFetch", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("returns parsed json for a successful response", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        new Response(JSON.stringify({ id: "1", displayName: "Тест" }), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      ),
    );

    await expect(apiFetch("/users/me")).resolves.toEqual({ id: "1", displayName: "Тест" });
  });

  it("throws ApiError with problem details for a failed response", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        problemResponse(400, { title: "InvalidDisplayName", status: 400, detail: "Недопустимое имя" }),
      ),
    );

    await expect(apiFetch("/users/me")).rejects.toMatchObject({
      status: 400,
      problem: { title: "InvalidDisplayName", detail: "Недопустимое имя" },
    });
  });

  it("marks 401 as unauthenticated", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () => problemResponse(401, { title: "not_authenticated", status: 401 })),
    );

    await expect(apiFetch("/users/me")).rejects.toMatchObject({ isUnauthenticated: true });
  });
});
