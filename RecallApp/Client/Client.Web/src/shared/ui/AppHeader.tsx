import { Anchor, Group, Title } from "@mantine/core";
import { Link } from "react-router";

/** Шапка приложения. Левый элемент, бренд, правый элемент. */
export function AppHeader({
  left,
  right,
}: {
  left?: React.ReactNode;
  right?: React.ReactNode;
}) {
  return (
    <Group h="100%" px="md" justify="space-between" wrap="nowrap">
      <Group gap="md" wrap="nowrap">
        {left}
        <Title order={4}>
          <Anchor component={Link} to="/" underline="never">
            RecallApp
          </Anchor>
        </Title>
      </Group>
      {right}
    </Group>
  );
}
