import { Menu } from "@mantine/core";

import { logoutUrl } from "@/features/session/model/sessionUrls";

/**
 * Пункт меню выхода.
 */
export function LogoutMenuItem() {
  return (
    <form method="post" action={logoutUrl}>
      <Menu.Item component="button" type="submit" w="100%">
        Выйти
      </Menu.Item>
    </form>
  );
}
