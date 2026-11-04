import { afterEach, describe, expect, it, vi } from "vitest";
import { createMemoryRouter, RouterProvider } from "react-router";
import { render } from "vitest-browser-react";

import { Providers } from "@/app/providers";
import { SessionMenu } from "./SessionMenu";

const authenticatedSession = {
  isAuthenticated: true,
  name: "token name",
  email: "token@recallapp.local",
};

const domainProfile = {
  id: "9d1c1a45-1a4d-4c4b-9a0e-9f3f7f0b2ad1",
  displayName: "Доменное Имя",
  email: "domain@recallapp.local",
  emailVerified: true,
};

/** Стаб ответов BFF: /session и /api/users/me отвечают по URL, остальное - 404. */
function stubBackend(routes: Record<string, { status: number; body: unknown }>) {
  vi.stubGlobal(
    "fetch",
    vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input instanceof Request ? input.url : input);
      const match = Object.entries(routes).find(([path]) => url.includes(path));
      if (!match) return new Response(null, { status: 404 });

      const { status, body } = match[1];
      return new Response(JSON.stringify(body), {
        status,
        headers: { "Content-Type": "application/json" },
      });
    }),
  );
}

/** Меню рендерится внутри провайдеров и роутера: нужен контекст для ссылки на аккаунт. */
async function renderMenu() {
  const router = createMemoryRouter([{ path: "/", element: <SessionMenu /> }], {
    initialEntries: ["/"],
  });

  return render(
    <Providers>
      <RouterProvider router={router} />
    </Providers>,
  );
}

describe("SessionMenu", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("shows only the login button when there is no session", async () => {
    stubBackend({ "/session": { status: 200, body: { isAuthenticated: false } } });

    const screen = await renderMenu();

    await expect.element(screen.getByRole("link", { name: "Войти" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Меню аккаунта" }).query()).toBeNull();
  });

  it("opens the account menu with the domain profile and account actions", async () => {
    stubBackend({
      "/session": { status: 200, body: authenticatedSession },
      "/users/me": { status: 200, body: domainProfile },
    });

    const screen = await renderMenu();
    await screen.getByRole("button", { name: "Меню аккаунта" }).click();

    await expect.element(screen.getByText("Доменное Имя").first()).toBeInTheDocument();
    await expect.element(screen.getByText("domain@recallapp.local")).toBeInTheDocument();
    await expect.element(screen.getByRole("menuitem", { name: "Аккаунт" })).toBeInTheDocument();
    await expect.element(screen.getByRole("menuitem", { name: "Выйти" })).toBeInTheDocument();
  });

  it("falls back to the token identity when the domain profile is unavailable", async () => {
    stubBackend({
      "/session": { status: 200, body: authenticatedSession },
      "/users/me": { status: 500, body: { title: "Unavailable", status: 500 } },
    });

    const screen = await renderMenu();
    await screen.getByRole("button", { name: "Меню аккаунта" }).click();

    await expect.element(screen.getByText("token name").first()).toBeInTheDocument();
    await expect.element(screen.getByText("token@recallapp.local")).toBeInTheDocument();
  });
});
