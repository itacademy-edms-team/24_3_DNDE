import { render, screen } from "@testing-library/react";
import { Button } from "@mantine/core";
import { describe, expect, it } from "vitest";

import { Providers } from "./providers";

// Smoke проверка связки Mantine + Providers в тестовом окружении.
describe("Providers", () => {
  it("renders a Mantine component", () => {
    render(
      <Providers>
        <Button>Начать</Button>
      </Providers>,
    );

    expect(screen.getByRole("button", { name: "Начать" })).toBeInTheDocument();
  });
});
