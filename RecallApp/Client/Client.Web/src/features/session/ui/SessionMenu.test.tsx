import { afterEach, describe, expect, it, vi } from "vitest";
import { createMemoryRouter, RouterProvider } from "react-router";
import { render } from "vitest-browser-react";

import { Providers } from "@/app/providers";
import { SessionMenu } from "./SessionMenu";

const authenticatedSession = {
  isAuthenticated: true,
  sub: "5fb9432d-f8f0-4eea-a7e7-cedd7daf8b4b",
  name: "test user",
  email: "test@recallapp.local",
  roles: [],
};

/** Стаб ответа BFF /session: единственный источник состояния для меню. */
function stubSession(body: unknown) {
  vi.stubGlobal(
    "fetch",
    vi.fn(
      async () =>
        new Response(JSON.stringify(body), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
    ),
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
    stubSession({ isAuthenticated: false });

    const screen = await renderMenu();

    await expect.element(screen.getByRole("link", { name: "Войти" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Меню аккаунта" }).query()).toBeNull();
  });

  it("opens the account menu with identity, account link and logout for an authenticated user", async () => {
    stubSession(authenticatedSession);

    const screen = await renderMenu();
    await screen.getByRole("button", { name: "Меню аккаунта" }).click();

    await expect.element(screen.getByText("test@recallapp.local")).toBeInTheDocument();
    await expect.element(screen.getByRole("menuitem", { name: "Аккаунт" })).toBeInTheDocument();
    await expect.element(screen.getByRole("menuitem", { name: "Выйти" })).toBeInTheDocument();
  });
});
