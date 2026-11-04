import { afterEach, describe, expect, it, vi } from "vitest";
import { page } from "vitest/browser";
import { createMemoryRouter, RouterProvider } from "react-router";
import { render } from "vitest-browser-react";

import { Providers } from "./providers";
import { routes } from "./routes";

/** Стаб ответа BFF /session для гостя. */
function stubAnonymousSession() {
  vi.stubGlobal(
    "fetch",
    vi.fn(async () =>
      new Response(JSON.stringify({ isAuthenticated: false }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      }),
    ),
  );
}

/** Рендерит приложение на заданном маршруте в провайдерах. */
async function renderAt(path: string) {
  const router = createMemoryRouter(routes, { initialEntries: [path] });
  return render(
    <Providers>
      <RouterProvider router={router} />
    </Providers>,
  );
}

/** Положение левой панели навигации: отрицательный x означает, что она за экраном. */
function navbarX(): number {
  const navbar = document.querySelector(".mantine-AppShell-navbar");
  expect(navbar).not.toBeNull();
  return (navbar as Element).getBoundingClientRect().x;
}

describe("routes", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("shows the home page content for a guest", async () => {
    stubAnonymousSession();
    const screen = await renderAt("/");

    await expect
      .element(screen.getByRole("heading", { name: "RecallApp", level: 1 }))
      .toBeInTheDocument();
    // Уникальный текст главной (в шапке его нет) - страница отрисована в каркасе.
    await expect
      .element(screen.getByText(/"Повторение - мать учения"/))
      .toBeInTheDocument();
  });

  it("keeps navigation off-canvas on mobile until the burger is used", async () => {
    stubAnonymousSession();
    await renderAt("/");

    // Mobile-first: панель навигации существует, но скрыта за левым краем.
    expect(navbarX()).toBeLessThan(0);

    await page.getByRole("button", { name: "Показать меню" }).click();

    await expect.element(page.getByRole("link", { name: "Главная" })).toBeVisible();
    expect(navbarX()).toBeGreaterThanOrEqual(0);
  });

  it("marks only the current route as active in the navigation", async () => {
    stubAnonymousSession();
    await renderAt("/");

    await expect
      .element(page.getByRole("link", { name: "Главная" }))
      .toHaveAttribute("aria-current", "page");
    await expect
      .element(page.getByRole("link", { name: "Аккаунт" }))
      .not.toHaveAttribute("aria-current", "page");
  });

  it("renders not found page for an unknown route", async () => {
    stubAnonymousSession();
    const screen = await renderAt("/no-such-page");

    await expect
      .element(screen.getByRole("heading", { name: "Страница не найдена" }))
      .toBeInTheDocument();
  });
});
