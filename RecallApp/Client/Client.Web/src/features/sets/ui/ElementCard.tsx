import { ActionIcon, Box, Card, Group, Menu } from "@mantine/core";
import {
  ArrowDownIcon,
  ArrowUpIcon,
  DotsSixVerticalIcon,
  GearSixIcon,
  TrashIcon,
} from "@phosphor-icons/react";

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
  /** Drag&drop перестановка (десктоп; на таче работают кнопки в меню). */
  isDragOver: boolean;
  onDragStart: (id: string) => void;
  onDragOver: (id: string) => void;
  onDrop: (id: string) => void;
  onDragEnd: () => void;
}

/** Оболочка карточки: общая «рама» (ручка DnD + меню действий) + поля конкретного типа. */
export function ElementCard({
  card,
  index,
  total,
  onCardChange,
  onBlur,
  onDelete,
  onMoveUp,
  onMoveDown,
  isDragOver,
  onDragStart,
  onDragOver,
  onDrop,
  onDragEnd,
}: ElementCardProps) {
  const canUp = index > 0 && !card.isDraft;
  const canDown = index < total - 1 && !card.isDraft;

  return (
    <Card
      withBorder
      radius="md"
      padding="md"
      style={
        isDragOver
          ? { outline: "2px solid var(--mantine-color-blue-5)" }
          : undefined
      }
      onDragOver={(event) => {
        event.preventDefault();
        onDragOver(card.id);
      }}
      onDrop={(event) => {
        event.preventDefault();
        onDrop(card.id);
      }}
    >
      <Group justify="space-between" align="center" mb="xs">
        <Box
          draggable={!card.isDraft}
          onDragStart={(event) => {
            event.dataTransfer.effectAllowed = "move";
            // Firefox не начинает drag без setData.
            event.dataTransfer.setData("text/plain", card.id);
            onDragStart(card.id);
          }}
          onDragEnd={onDragEnd}
          title="Перетащите, чтобы изменить порядок"
          c="dimmed"
          style={{
            cursor: card.isDraft ? "default" : "grab",
            display: "flex",
            padding: 4,
          }}
        >
          <DotsSixVerticalIcon size={20} />
        </Box>

        <Menu shadow="md" position="bottom-end" width={200}>
          <Menu.Target>
            <ActionIcon
              variant="subtle"
              color="gray"
              aria-label="Действия с карточкой"
            >
              <GearSixIcon size={20} />
            </ActionIcon>
          </Menu.Target>
          <Menu.Dropdown>
            <Menu.Item
              leftSection={<ArrowUpIcon size={16} />}
              disabled={!canUp}
              onClick={onMoveUp}
            >
              Вверх
            </Menu.Item>
            <Menu.Item
              leftSection={<ArrowDownIcon size={16} />}
              disabled={!canDown}
              onClick={onMoveDown}
            >
              Вниз
            </Menu.Item>
            <Menu.Item
              leftSection={<TrashIcon size={16} />}
              color="red"
              onClick={onDelete}
            >
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
