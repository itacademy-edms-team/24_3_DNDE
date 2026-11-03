import { afterEach, describe, expect, it, vi } from "vitest";
import { createMemoryRouter, RouterProvider } from "react-router";
import { render } from "vitest-browser-react";

import { Providers } from "./providers";
import { routes } from "./routes";

/** Стаб ответа BFF /session для гостя. */
function stubAnonymousSession() {
  vi.stubGlobal(
    "fetch",
    vi.fn(
      async () =>
        new Response(JSON.stringify({ isAuthenticated: false }), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
    ),
  );
}

describe("routes", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("shows the login action on the home page for a guest", async () => {
    stubAnonymousSession();

    const router = createMemoryRouter(routes, { initialEntries: ["/"] });
    const screen = await render(
      <Providers>
        <RouterProvider router={router} />
      </Providers>,
    );

    await expect
      .element(screen.getByRole("heading", { name: "RecallApp", level: 1 }))
      .toBeInTheDocument();
    // Уникальный текст главной (в шапке его нет) - страница отрисована в каркасе.
    await expect
      .element(screen.getByText(/"Повторение - мать учения"/))
      .toBeInTheDocument();
  });

  it("renders not found page for an unknown route", async () => {
    stubAnonymousSession();

    const router = createMemoryRouter(routes, {
      initialEntries: ["/no-such-page"],
    });
    const screen = await render(
      <Providers>
        <RouterProvider router={router} />
      </Providers>,
    );

    await expect
      .element(screen.getByRole("heading", { name: "Страница не найдена" }))
      .toBeInTheDocument();
  });
});
