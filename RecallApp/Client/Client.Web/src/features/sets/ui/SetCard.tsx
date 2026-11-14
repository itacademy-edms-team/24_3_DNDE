import {
  ActionIcon,
  Card,
  Group,
  Menu,
  Stack,
  Text,
  UnstyledButton,
} from "@mantine/core";
import {
  DotsThreeVerticalIcon,
  EyeIcon,
  PencilSimpleIcon,
  TrashIcon,
} from "@phosphor-icons/react";
import { Link } from "react-router";

import type { SetSummary } from "@/features/sets/model/types";
import { formatUpdatedAt } from "./formatUpdatedAt";

interface SetCardProps {
  set: SetSummary;
  onRename: (set: SetSummary) => void;
  onDelete: (set: SetSummary) => void;
}

/** Карточка набора: клик по содержимому открывает редактор, меню — переименовать/удалить. */
export function SetCard({ set, onRename, onDelete }: SetCardProps) {
  return (
    <Card withBorder radius="md" padding="lg">
      <Group justify="space-between" align="flex-start" wrap="nowrap">
        <UnstyledButton
          component={Link}
          to={`/sets/${set.id}/edit`}
          style={{ flex: 1, minWidth: 0, textAlign: "left" }}
        >
          <Stack gap={2}>
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
        </UnstyledButton>

        <Menu shadow="md" position="bottom-end" width={200}>
          <Menu.Target>
            <ActionIcon
              variant="subtle"
              color="gray"
              aria-label="Действия с набором"
            >
              <DotsThreeVerticalIcon size={20} />
            </ActionIcon>
          </Menu.Target>
          <Menu.Dropdown>
            <Menu.Item
              leftSection={<EyeIcon size={16} />}
              component={Link}
              to={`/sets/${set.id}/preview`}
            >
              Предпросмотр
            </Menu.Item>
            <Menu.Item
              leftSection={<PencilSimpleIcon size={16} />}
              onClick={() => onRename(set)}
            >
              Переименовать
            </Menu.Item>
            <Menu.Item
              leftSection={<TrashIcon size={16} />}
              color="red"
              onClick={() => onDelete(set)}
            >
              Удалить
            </Menu.Item>
          </Menu.Dropdown>
        </Menu>
      </Group>
    </Card>
  );
}
