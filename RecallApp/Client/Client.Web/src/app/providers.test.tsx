import { Button } from "@mantine/core";
import { describe, expect, it } from "vitest";
import { render } from "vitest-browser-react";

import "../styles/app.css";
import { Providers } from "./providers";

// Дымовая проверка связки Mantine + провайдеры в реальном браузере.
describe("Providers", () => {
  it("renders a Mantine component inside the app providers", async () => {
    const screen = await render(
      <Providers>
        <Button>Начать</Button>
      </Providers>,
    );

    await expect
      .element(screen.getByRole("button", { name: "Начать" }))
      .toBeInTheDocument();
  });

  it("applies Mantine and Tailwind styles", async () => {
    const screen = await render(
      <Providers>
        <Button>Начать</Button>
      </Providers>,
    );

    const button = screen.getByRole("button", { name: "Начать" }).element();
    const styles = window.getComputedStyle(button);

    // Mantine-переменные и заливка кнопки появляются только если CSS реально подключён.
    expect(
      window
        .getComputedStyle(document.documentElement)
        .getPropertyValue("--mantine-color-blue-6")
        .trim(),
    ).not.toBe("");
    expect(styles.backgroundColor).not.toBe("rgba(0, 0, 0, 0)");
  });
});
