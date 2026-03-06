import { ComponentType, StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

// from MUI's toolpad we only use Notifications
import { NotificationsProvider } from '@toolpad/core/useNotifications';
import { Provider as JotaiProvider } from 'jotai';

import ThemeProvider from '@/theme/Provider';

import {
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query'

const queryClient = new QueryClient()

const container = document.getElementById('root') as HTMLElement;
const root = createRoot(container);

function render(App: ComponentType) {
  root.render(
    <QueryClientProvider client={queryClient}>
      <StrictMode>
        <JotaiProvider>
          <ThemeProvider>
            <NotificationsProvider>
              <App />
            </NotificationsProvider>
          </ThemeProvider>
        </JotaiProvider>
      </StrictMode>
    </QueryClientProvider>,
  );
}

export default render;
