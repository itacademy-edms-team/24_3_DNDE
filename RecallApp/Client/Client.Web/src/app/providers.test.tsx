import { describe, expect, it } from "vitest";
import { render } from "vitest-browser-react";

import { LoginButton } from "../features/session/ui/LoginButton";
import "../styles/app.css";
import { Providers } from "./providers";

// Smoke test связки Mantine + провайдеры в реальном браузере.
describe("Providers", () => {
  it("renders a Mantine component inside the app providers", async () => {
    const screen = await render(
      <Providers>
        <LoginButton />
      </Providers>,
    );

    await expect
      .element(screen.getByRole("link", { name: "Войти" }))
      .toBeInTheDocument();
  });

  it("applies Mantine and Tailwind styles", async () => {
    const screen = await render(
      <Providers>
        <LoginButton />
      </Providers>,
    );

    const link = screen.getByRole("link", { name: "Войти" }).element();
    const styles = window.getComputedStyle(link);

    // Mantine-переменные и заливка кнопки появляются только если CSS действительно подключён.
    expect(
      window
        .getComputedStyle(document.documentElement)
        .getPropertyValue("--mantine-color-blue-6")
        .trim(),
    ).not.toBe("");
    expect(styles.backgroundColor).not.toBe("rgba(0, 0, 0, 0)");
  });
});
