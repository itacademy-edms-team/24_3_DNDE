/// <reference types="vitest/config" />
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { playwright } from "@vitest/browser-playwright";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  test: {
    // Все тесты клиента идут в реальном браузере (Playwright под капотом Vitest):
    // настоящие CSS-слои Mantine/Tailwind, layout и медиа-запросы, без jsdom-эмуляции.
    // Перед первым запуском: npx playwright install chromium.
    include: ["src/**/*.test.{ts,tsx}"],

    browser: {
      enabled: true,
      provider: playwright(),
      instances: [{ browser: "chromium" }],
    },
  },
});
