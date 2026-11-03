import { Button } from "@mantine/core";

import { loginUrl } from "../model/sessionUrls";

/** Кнопка входа: обычная навигация на BFF, откуда уходим на страницу входа Keycloak. */
export function LoginButton() {
  return (
    <Button component="a" href={loginUrl()} variant="filled">
      Войти
    </Button>
  );
}
