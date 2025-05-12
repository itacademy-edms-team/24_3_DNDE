import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin()],
  server: {
    https: {
      key: fs.readFileSync('./certs/localhost-key.pem'),
      cert: fs.readFileSync('./certs/localhost-cert.pem'),
    },
    host: 'localhost',
    port: 10048,
    strictPort: true,
  },
})
