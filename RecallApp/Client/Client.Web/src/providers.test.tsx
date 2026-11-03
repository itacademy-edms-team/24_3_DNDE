import { describe, expect, it } from "vitest";
import { render } from "vitest-browser-react";

import App from "./App";
import { Providers } from "./providers";

// Проверка связки Mantine + провайдеры в реальном браузере (CSS-слои, layout, matchMedia).
describe("Providers", () => {
  it("renders a Mantine component", async () => {
    const screen = await render(
      <Providers>
        <App />
      </Providers>,
    );

    await expect.element(screen.getByRole("heading", { name: "RecallApp" })).toBeInTheDocument();
  });
});
