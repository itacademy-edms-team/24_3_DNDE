import { Button, Divider, Stack, Text } from "@mantine/core";
import { Link } from "react-router";

import { useSession } from "@/features/session/api/useSession";
import { LoginButton } from "./LoginButton";
import { LogoutButton } from "./LogoutButton";

/**
 * Правая боковая панель.
 */
export function SessionPanel({ onNavigate }: { onNavigate?: () => void }) {
  const { data: session } = useSession();
  const isAuthenticated = session?.isAuthenticated === true;

  if (!isAuthenticated) {
    return (
      <Stack gap="sm">
        <Text fw={600}>Гость</Text>
        <Text size="xs" c="dimmed">
          Войдите, чтобы сохранять наборы и прогресс.
        </Text>
        <LoginButton />
      </Stack>
    );
  }

  return (
    <Stack gap="sm">
      <Stack gap={2}>
        <Text fw={600}>{session.name ?? session.email ?? "Пользователь"}</Text>
        {session.email && (
          <Text size="xs" c="dimmed">
            {session.email}
          </Text>
        )}
      </Stack>
      <Divider />
      <Button
        component={Link}
        to="/account"
        variant="light"
        onClick={onNavigate}
      >
        Аккаунт
      </Button>
      <LogoutButton />
    </Stack>
  );
}
