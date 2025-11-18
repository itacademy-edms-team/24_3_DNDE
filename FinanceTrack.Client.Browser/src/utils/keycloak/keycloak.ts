import Keycloak from 'keycloak-js';

const keycloak = new Keycloak({
  url: 'http://localhost:5000/keycloak',
  realm: 'FinanceTrack',
  clientId: 'FinanceTrack-react',
});

export const keycloakInitOptions = {
  onLoad: 'check-sso',
  checkLoginIframe: false, // Disable wrong working default auth check. Going to implement own
};

export default keycloak;
