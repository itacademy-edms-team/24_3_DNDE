import { Badge, Card, Group, Stack, Text } from "@mantine/core";

import type { UserProfile } from "../model/types";

/** Карточка профиля: доменное имя, email и статус подтверждения адреса. */
export function ProfileCard({ profile }: { profile: UserProfile }) {
  return (
    <Card withBorder radius="md" padding="lg" maw={480}>
      <Stack gap="xs">
        <Group justify="space-between">
          <Text fw={600} size="lg">
            {profile.displayName}
          </Text>
          <Badge color={profile.emailVerified ? "teal" : "yellow"} variant="light">
            {profile.emailVerified ? "email подтверждён" : "email не подтверждён"}
          </Badge>
        </Group>
        <Text size="sm" c="dimmed">
          {profile.email}
        </Text>
        <Text size="xs" c="dimmed">
          id: {profile.id}
        </Text>
      </Stack>
    </Card>
  );
}
