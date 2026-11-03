import { Button } from "@mantine/core";

import { logoutUrl } from "../model/sessionUrls";

/**
 * Кнопка выхода: отправка HTML-формы на BFF (POST /logout).
 * Через fetch нельзя: кросс-origin редирект на Keycloak блокируется CORS.
 */
export function LogoutButton() {
  return (
    <form method="post" action={logoutUrl}>
      <Button type="submit" variant="light">
        Выйти
      </Button>
    </form>
  );
}
