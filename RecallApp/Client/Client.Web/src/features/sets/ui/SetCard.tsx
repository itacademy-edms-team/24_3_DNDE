import { ActionIcon, Card, Group, Menu, Stack, Text } from "@mantine/core";
import { DotsThreeVertical, PencilSimple, Trash } from "@phosphor-icons/react";

import type { SetSummary } from "@/features/sets/model/types";
import { formatUpdatedAt } from "./formatUpdatedAt";

interface SetCardProps {
  set: SetSummary;
  onEdit: (set: SetSummary) => void;
  onDelete: (set: SetSummary) => void;
}

/** Карточка набора в списке: название, описание, дата изменения и меню действий. */
export function SetCard({ set, onEdit, onDelete }: SetCardProps) {
  return (
    <Card withBorder radius="md" padding="lg">
      <Group justify="space-between" align="flex-start" wrap="nowrap">
        <Stack gap={2} style={{ minWidth: 0, flex: 1 }}>
          <Text fw={600} truncate>
            {set.title}
          </Text>
          {set.description && (
            <Text size="sm" c="dimmed" lineClamp={2}>
              {set.description}
            </Text>
          )}
          <Text size="xs" c="dimmed">
            Изменено {formatUpdatedAt(set.updatedAt)}
          </Text>
        </Stack>

        <Menu shadow="md" position="bottom-end" width={200}>
          <Menu.Target>
            <ActionIcon variant="subtle" color="gray" aria-label="Действия с набором">
              <DotsThreeVertical size={20} />
            </ActionIcon>
          </Menu.Target>
          <Menu.Dropdown>
            <Menu.Item leftSection={<PencilSimple size={16} />} onClick={() => onEdit(set)}>
              Редактировать
            </Menu.Item>
            <Menu.Item leftSection={<Trash size={16} />} color="red" onClick={() => onDelete(set)}>
              Удалить
            </Menu.Item>
          </Menu.Dropdown>
        </Menu>
      </Group>
    </Card>
  );
}
