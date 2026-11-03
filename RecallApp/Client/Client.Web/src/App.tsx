import { Button, Center, Stack, Text, Title } from "@mantine/core";

/**
 * Заглушка главной страницы; реальные страницы и layout появятся на этапе структуры папок.
 */
export default function App() {
  return (
    <Center h="100vh">
      <Stack align="center" gap="xs">
        <Title order={1}>RecallApp</Title>
        <Text c="dimmed">Скелет клиента собран</Text>
        <Button component="a" href="https://mantine.dev" target="_blank" variant="light">
          Mantine работает
        </Button>
      </Stack>
    </Center>
  );
}
