import { ComponentType, StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

// from MUI's toolpad we only use Notifications
import { NotificationsProvider } from '@toolpad/core/useNotifications';
import { Provider as JotaiProvider } from 'jotai';

import ThemeProvider from '@/theme/Provider';
import { ReactKeycloakProvider } from '@react-keycloak/web';
import keycloack from '@/utils/keycloak/keycloak';

const container = document.getElementById('root') as HTMLElement;
const root = createRoot(container);

function render(App: ComponentType) {
  root.render(
    <StrictMode>
      <ReactKeycloakProvider authClient={keycloack}>
        <JotaiProvider>
          <ThemeProvider>
            <NotificationsProvider>
              <App />
            </NotificationsProvider>
          </ThemeProvider>
        </JotaiProvider>
      </ReactKeycloakProvider>
    </StrictMode>,
  );
}

export default render;
