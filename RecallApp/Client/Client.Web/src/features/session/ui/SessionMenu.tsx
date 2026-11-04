import { Avatar, Group, Menu, Text, UnstyledButton } from "@mantine/core";
import { Link } from "react-router";

import { useSession } from "@/features/session/api/useSession";
import { LoginButton } from "./LoginButton";
import { LogoutMenuItem } from "./LogoutMenuItem";

/**
 * Сессия в шапке: гость видит кнопку входа, вошедший — меню под именем
 * (аккаунт, выход). Отдельная панель справа не нужна: два действия не стоят
 * постоянного места на экране, особенно на мобильном.
 */
export function SessionMenu() {
  const { data: session } = useSession();

  if (session?.isAuthenticated !== true) {
    return <LoginButton />;
  }

  const displayName = session.name ?? session.email ?? "Пользователь";

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
          {session.email && (
            <Text size="xs" c="dimmed" truncate>
              {session.email}
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
