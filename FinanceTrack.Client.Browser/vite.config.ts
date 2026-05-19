import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vite';
import { VitePWA } from 'vite-plugin-pwa';

import manifest from './manifest.json';

// https://vite.dev/config/
export default defineConfig(() => {
  return {
    server: {
      host: true,
      port: 5173,
      strictPort: true,
      allowedHosts: ['client.browser', 'app.ft.localhost'],
    },
    preview: {
      host: true,
      port: 5173,
      strictPort: true,
    },
    plugins: [
      react(),
      VitePWA({
        manifest,
        includeAssets: ['favicon.svg', 'favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
        // switch to "true" to enable sw on development
        devOptions: { enabled: false },
        registerType: 'autoUpdate',
        workbox: {
          globPatterns: ['**/*.{js,css,html}', '**/*.{svg,png,jpg,gif}'],
          // Let BFF and API requests bypass the SW and go straight to the server
          navigateFallbackDenylist: [/^\/bff/, /^\/api/],
        },
      }),
    ],
    resolve: { alias: { '@': path.resolve(__dirname, './src') } },
  };
});
