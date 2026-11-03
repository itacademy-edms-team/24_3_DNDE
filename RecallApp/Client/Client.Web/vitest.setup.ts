import "@testing-library/jest-dom/vitest";

// Мок window.matchMedia для @mantine/hooks (use-media-query и компоненты Mantine).
// См. https://mantine.dev/guides/vitest/
Object.defineProperty(window, "matchMedia", {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }),
});
