import { ActionIcon, Card, Group, Menu } from "@mantine/core";
import { ArrowDown, ArrowUp, GearSix, Trash } from "@phosphor-icons/react";

import { FlashcardFields } from "./FlashcardFields";
import type { FlashcardDraft } from "./flashcard";

/**
 * Карточка в редакторе — размеченное объединение по `type`.
 * Новый тип = добавить вариант сюда + ветку в dispatch ниже + модуль полей типа.
 */
export type EditorCard = {
  id: string;
  isDraft: boolean;
  type: "flashcard";
  payload: FlashcardDraft;
};

interface ElementCardProps {
  card: EditorCard;
  index: number;
  total: number;
  /** Замена карточки целиком: поля конкретного типа собирают новый payload. */
  onCardChange: (card: EditorCard) => void;
  onBlur: () => void;
  onDelete: () => void;
  onMoveUp: () => void;
  onMoveDown: () => void;
}

/** Оболочка карточки: общая «рама» (меню действий) + поля конкретного типа. */
export function ElementCard({
  card,
  index,
  total,
  onCardChange,
  onBlur,
  onDelete,
  onMoveUp,
  onMoveDown,
}: ElementCardProps) {
  const canUp = index > 0 && !card.isDraft;
  const canDown = index < total - 1 && !card.isDraft;

  return (
    <Card withBorder radius="md" padding="md">
      <Group justify="flex-end" mb="xs">
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

      {card.type === "flashcard" ? (
        <FlashcardFields
          draft={card.payload}
          onChange={(draft) => onCardChange({ ...card, payload: draft })}
          onBlur={onBlur}
        />
      ) : null}
    </Card>
  );
}
