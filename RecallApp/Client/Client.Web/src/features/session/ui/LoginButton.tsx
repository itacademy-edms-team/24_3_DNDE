import { Button } from "@mantine/core";
import { useLocation } from "react-router";

import { loginUrl } from "../model/sessionUrls";

/** Кнопка входа: обычная навигация на BFF, откуда уходим на страницу входа Keycloak. */
export function LoginButton() {
  const location = useLocation();
  const returnTo = `${location.pathname}${location.search}`;

  return (
    <Button component="a" href={loginUrl(returnTo)} variant="filled">
      Войти
    </Button>
  );
}
