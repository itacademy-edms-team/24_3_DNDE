import { afterEach, describe, expect, it, vi } from "vitest";
import { createMemoryRouter, RouterProvider } from "react-router";
import { render } from "vitest-browser-react";

import { Providers } from "@/app/providers";
import { SessionPanel } from "./SessionPanel";

/** Стаб ответа BFF /session для вошедшего пользователя. */
function stubAuthenticatedSession() {
  vi.stubGlobal(
    "fetch",
    vi.fn(async () =>
      new Response(
        JSON.stringify({
          isAuthenticated: true,
          sub: "5fb9432d-f8f0-4eea-a7e7-cedd7daf8b4b",
          name: "test user",
          email: "test@recallapp.local",
          roles: [],
        }),
        { status: 200, headers: { "Content-Type": "application/json" } },
      ),
    ),
  );
}

/** Рендерит панель внутри провайдеров и роутера (нужен контекст для ссылки на аккаунт). */
async function renderPanel() {
  const router = createMemoryRouter([{ path: "/", element: <SessionPanel /> }], {
    initialEntries: ["/"],
  });

  return render(
    <Providers>
      <RouterProvider router={router} />
    </Providers>,
  );
}

describe("SessionPanel", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("shows the user identity and account actions for an authenticated user", async () => {
    stubAuthenticatedSession();
    const screen = await renderPanel();

    await expect.element(screen.getByText("test@recallapp.local")).toBeInTheDocument();
    await expect.element(screen.getByRole("link", { name: "Аккаунт" })).toBeInTheDocument();
    await expect.element(screen.getByRole("button", { name: "Выйти" })).toBeInTheDocument();
  });

  it("shows the guest invitation when there is no session", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        new Response(JSON.stringify({ isAuthenticated: false }), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      ),
    );

    const screen = await renderPanel();

    await expect.element(screen.getByText("Гость")).toBeInTheDocument();
    await expect.element(screen.getByRole("link", { name: "Войти" })).toBeInTheDocument();
  });
});
