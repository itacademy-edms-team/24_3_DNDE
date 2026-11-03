import { Anchor, Container, Group, Title } from "@mantine/core";
import { Link } from "react-router";

/** Каркас шапки: бренд, навигация и слот действий. Не знает о фичах и доменах. */
export function AppHeader({ actions }: { actions?: React.ReactNode }) {
  return (
    <Container size="lg" py="sm">
      <Group justify="space-between">
        <Group gap="lg">
          <Title order={4}>
            <Anchor component={Link} to="/" underline="never">
              RecallApp
            </Anchor>
          </Title>
          <Anchor component={Link} to="/account" size="sm" c="dimmed">
            Аккаунт
          </Anchor>
        </Group>
        {actions}
      </Group>
    </Container>
  );
}
