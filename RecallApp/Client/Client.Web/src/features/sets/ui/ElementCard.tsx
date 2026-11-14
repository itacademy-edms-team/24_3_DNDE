import { ActionIcon, Box, Card, Group, Menu } from "@mantine/core";
import { useSortable } from "@dnd-kit/sortable";
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
}

/**
 * Оболочка карточки: общая «рама» (ручка DnD + меню действий) + поля конкретного типа.
 * Перестановка — через @dnd-kit/sortable (Pointer/Keyboard-сенсоры), работает мышью и пальцем.
 */
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
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: card.id,
    disabled: card.isDraft,
  });

  const canUp = index > 0 && !card.isDraft;
  const canDown = index < total - 1 && !card.isDraft;

  return (
    <Card
      ref={setNodeRef}
      withBorder
      radius="md"
      padding="md"
      style={{
        transform: transform
          ? `translate3d(${transform.x}px, ${transform.y}px, 0)`
          : undefined,
        transition,
        opacity: isDragging ? 0.6 : undefined,
        position: "relative",
        zIndex: isDragging ? 1 : undefined,
      }}
    >
      <Group justify="space-between" align="center" mb="xs">
        <Box
          {...attributes}
          {...listeners}
          title="Перетащите, чтобы изменить порядок"
          c="dimmed"
          style={{
            cursor: card.isDraft ? "default" : "grab",
            display: "flex",
            padding: 4,
            // Без touch-action браузер перехватывает жест под скролл — DnD на таче не стартует.
            touchAction: "none",
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
