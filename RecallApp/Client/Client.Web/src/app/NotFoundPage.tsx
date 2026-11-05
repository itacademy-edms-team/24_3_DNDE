import { Button, Center, Stack, Text, Title } from "@mantine/core";
import { Link } from "react-router";

/** Страница «не найдено» для неизвестных маршрутов. */
export function NotFoundPage() {
  return (
    <Center h="50vh">
      <Stack align="center" gap="sm">
        <Title order={2}>Страница не найдена</Title>
        <Text c="dimmed">Такого адреса в приложении нет.</Text>
        <Button component={Link} to="/" variant="light">
          На главную
        </Button>
      </Stack>
    </Center>
  );
}
