import {
  ActionIcon,
  Card,
  Divider,
  Group,
  Menu,
  Text,
  Textarea,
  TextInput,
} from "@mantine/core";
import { ArrowDown, ArrowUp, GearSix, Trash } from "@phosphor-icons/react";

/** Карточка в редакторе: сохранённый элемент или локальный черновик (ещё не создан на сервере). */
export interface EditorCard {
  /** uuid сервера либо локальный id черновика. */
  id: string;
  isDraft: boolean;
  term: string;
  definition: string;
  frontImageId: string | null;
  backImageId: string | null;
}

interface ElementCardProps {
  card: EditorCard;
  index: number;
  total: number;
  onTermChange: (value: string) => void;
  onDefinitionChange: (value: string) => void;
  onBlur: () => void;
  onDelete: () => void;
  onMoveUp: () => void;
  onMoveDown: () => void;
}

/** Одна карточка редактора: лицевая (термин) / оборотная (определение) + меню действий. */
export function ElementCard({
  card,
  index,
  total,
  onTermChange,
  onDefinitionChange,
  onBlur,
  onDelete,
  onMoveUp,
  onMoveDown,
}: ElementCardProps) {
  // Черновик ещё не имеет позиции на сервере — его нельзя переставлять.
  const canUp = index > 0 && !card.isDraft;
  const canDown = index < total - 1 && !card.isDraft;

  return (
    <Card withBorder radius="md" padding="md">
      <Group justify="space-between" align="center" mb="xs">
        <Text size="xs" c="dimmed" tt="uppercase" fw={700}>
          Лицевая сторона
        </Text>
        <Menu shadow="md" position="bottom-end" width={200}>
          <Menu.Target>
            <ActionIcon variant="subtle" color="gray" aria-label="Действия с карточкой">
              <GearSix size={20} />
            </ActionIcon>
          </Menu.Target>
          <Menu.Dropdown>
            <Menu.Item leftSection={<ArrowUp size={16} />} disabled={!canUp} onClick={onMoveUp}>
              Вверх
            </Menu.Item>
            <Menu.Item leftSection={<ArrowDown size={16} />} disabled={!canDown} onClick={onMoveDown}>
              Вниз
            </Menu.Item>
            <Menu.Item leftSection={<Trash size={16} />} color="red" onClick={onDelete}>
              Удалить
            </Menu.Item>
          </Menu.Dropdown>
        </Menu>
      </Group>

      <TextInput
        placeholder="Термин"
        value={card.term}
        onChange={(event) => onTermChange(event.currentTarget.value)}
        onBlur={onBlur}
      />

      <Divider my="md" />

      <Text size="xs" c="dimmed" tt="uppercase" fw={700} mb="xs">
        Оборотная сторона
      </Text>
      <Textarea
        placeholder="Определение"
        value={card.definition}
        onChange={(event) => onDefinitionChange(event.currentTarget.value)}
        onBlur={onBlur}
        minRows={3}
        autosize
      />
    </Card>
  );
}
