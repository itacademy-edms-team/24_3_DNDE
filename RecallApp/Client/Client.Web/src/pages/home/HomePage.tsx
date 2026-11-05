import { Button, Center, Stack, Text, Title } from "@mantine/core";
import { Link } from "react-router";

import { useSession } from "@/features/session/api/useSession";
import { LoginButton } from "@/features/session/ui/LoginButton";

/** Главная страница */
export function HomePage() {
  const { data: session } = useSession();
  const isAuthenticated = session?.isAuthenticated === true;

  return (
    <Center h="50vh">
      <Stack align="center" gap="sm" maw={520}>
        <Title order={1}>RecallApp</Title>
        <Text c="dimmed" ta="center">
          "Повторение - мать учения"
        </Text>
        {isAuthenticated ? (
          <Button component={Link} to="/account" variant="filled">
            Мой аккаунт
          </Button>
        ) : (
          <LoginButton />
        )}
      </Stack>
    </Center>
  );
}
