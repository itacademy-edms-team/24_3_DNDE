import { Avatar, Group, Menu, Text, UnstyledButton } from "@mantine/core";
import { Link } from "react-router";

import { useMyAccount } from "@/features/account/api/useMyAccount";
import { useSession } from "@/features/session/api/useSession";
import { LoginButton } from "./LoginButton";
import { LogoutMenuItem } from "./LogoutMenuItem";

/**
 * Компонент меню аккаунта в шапке.
 */
export function SessionMenu() {
  const { data: session } = useSession();
  const { data: profile } = useMyAccount();

  if (session?.isAuthenticated !== true) {
    return <LoginButton />;
  }

  // Именем владеет домен (ADR 0020), токен - только фоллбэк: пока профиль не загружен
  // или монолит недоступен, шапка показывает имя из ID-токена.
  const displayName =
    profile?.displayName ?? session.name ?? session.email ?? "Пользователь";
  const email = profile?.email ?? session.email;

  return (
    <Menu position="bottom-end" width={220} shadow="md" withinPortal>
      <Menu.Target>
        <UnstyledButton aria-label="Меню аккаунта" p={4}>
          <Group gap="xs" wrap="nowrap">
            <Avatar size={28} radius="xl" color="blue">
              {displayName.slice(0, 1).toUpperCase()}
            </Avatar>
            <Text size="sm" fw={500} visibleFrom="sm" maw={160} truncate>
              {displayName}
            </Text>
            <Text size="xs" c="dimmed" aria-hidden>
              ▾
            </Text>
          </Group>
        </UnstyledButton>
      </Menu.Target>

      <Menu.Dropdown>
        <Menu.Label>
          <Text size="sm" fw={500} truncate>
            {displayName}
          </Text>
          {email && (
            <Text size="xs" c="dimmed" truncate>
              {email}
            </Text>
          )}
        </Menu.Label>

        <Menu.Item component={Link} to="/account">
          Аккаунт
        </Menu.Item>
        <Menu.Divider />
        <LogoutMenuItem />
      </Menu.Dropdown>
    </Menu>
  );
}
