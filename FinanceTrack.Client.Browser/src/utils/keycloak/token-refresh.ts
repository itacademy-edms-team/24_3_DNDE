import type Keycloak from 'keycloak-js';

interface KeycloakTokens {
  idToken?: string;
  refreshToken?: string;
  token?: string;
}

let refreshTimeoutId: number | undefined;
const SAFETY_WINDOW_SECONDS = 30; // update when timeout bello 30 secs

function clearRefreshTimeout() {
  if (refreshTimeoutId !== undefined) {
    console.log('[Keycloak] No token received, clearing refresh timeout');
    window.clearTimeout(refreshTimeoutId);
    refreshTimeoutId = undefined;
  }
}

export function handleTokens(keycloak: Keycloak, tokens: KeycloakTokens | null) {
  // No token - clear timer and exit
  if (!tokens || !tokens.token) {
    console.log('[Keycloak] tokenParsed or exp missing, clearing refresh timeout');
    clearRefreshTimeout();
    return;
  }

  const tokenParsed = keycloak.tokenParsed;
  if (!tokenParsed || !tokenParsed.exp) {
    clearRefreshTimeout();
    return;
  }

  // exp — unix time in seconds
  const expiresAtMs = tokenParsed.exp * 1000;
  const nowMs = Date.now();

  // How long before expiry should we try to renew the token
  const refreshAtMs = expiresAtMs - SAFETY_WINDOW_SECONDS * 1000;

  // If refreshAt is already in the past, we try to refresh immediately
  const delayMs = Math.max(refreshAtMs - nowMs, 0);

  //   console.log('[Keycloak] New token', {
  //     exp: new Date(expiresAtMs).toISOString(),
  //     now: new Date(nowMs).toISOString(),
  //     refreshInSec: Math.round(delayMs / 1000),
  //   });

  clearRefreshTimeout();

  refreshTimeoutId = window.setTimeout(async () => {
    console.log('[Keycloak] Trying to refresh token...');
    try {
      // Renew the token if less than SAFETY_WINDOW_SECONDS remain before expiry
      const refreshed = await keycloak.updateToken(SAFETY_WINDOW_SECONDS);
      console.log('[Keycloak] updateToken result:', refreshed);
      if (!refreshed) {
        console.log('[Keycloak] Token still valid, no refresh was needed');
        // Token still alive, doing nothing
        // onTokens will be called up during the next update, if there is one
        return;
      }
      // Successfully updated — ReactKeycloakProvider will call onTokens again itself.
      // and then will reset the timer with new exp.
    } catch (err) {
      console.warn('[Keycloak] Failed to refresh token, logging out', err);
      // Write custom logic down here if need
      keycloak.logout();
    }
  }, delayMs);
}
