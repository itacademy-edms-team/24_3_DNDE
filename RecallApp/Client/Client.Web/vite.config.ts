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
    // Каждый тестовый файл получает стили приложения через общий setup.
    setupFiles: ["./src/test/setup.ts"],
    // По умолчанию Vitest подменяет CSS пустыми строками;
    // в браузерном режиме нужны настоящие стили Mantine/Tailwind, без них тесты не будут работать корректно.
    css: true,
    browser: {
      enabled: true,
      provider: playwright(),
      instances: [{ browser: "chromium" }],
      // Размер окна браузера, в котором будут запускаться тесты. Решение mobile-first, поэтому и параметры должны быть под мобилки.
      viewport: { width: 390, height: 844 },
    },
  },
});
