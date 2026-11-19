import { useEffect, useRef, useState } from "react";
import {
  Alert,
  Button,
  Center,
  Group,
  Loader,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import {
  closestCenter,
  DndContext,
  type DragEndEvent,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { ArrowLeft, Plus } from "@phosphor-icons/react";
import { Link, useParams } from "react-router";

import { toErrorView } from "@/shared/api/errors/errorView";
import { RequireSession } from "@/features/session/ui/RequireSession";
import { useSet } from "@/features/sets/api/useSet";
import { useElements } from "@/features/sets/api/useElements";
import {
  useCreateElement,
  useDeleteElement,
  useMoveElement,
  useUpdateElement,
} from "@/features/sets/api/useElementMutations";
import { ElementCard, type EditorCard } from "@/features/sets/ui/ElementCard";
import { flashcard } from "@/features/sets/ui/flashcard";
import { useMediaLinks } from "@/features/media/api/useMediaLinks";
import type { Element, ElementPayloadInput } from "@/features/sets/model/types";

const AUTOSAVE_DELAY = 600;

// Dispatch-точки по типу элемента. Новый тип = добавить ветку (switch исчерпывающий, TS заставит).
function toEditorCard(element: Element): EditorCard {
  switch (element.type) {
    case "flashcard":
      return {
        id: element.id,
        isDraft: false,
        type: "flashcard",
        payload: flashcard.toDraft(element.payload, element.images),
      };
  }
}

function toApiPayload(card: EditorCard): ElementPayloadInput {
  switch (card.type) {
    case "flashcard":
      return flashcard.toApiPayload(card.payload);
  }
}

function isComplete(card: EditorCard): boolean {
  switch (card.type) {
    case "flashcard":
      return flashcard.isComplete(card.payload);
  }
}

/** Файлы, на которые ссылаются карточки: по ним переподписываются ссылки редактора. */
function cardMediaIds(card: EditorCard): string[] {
  switch (card.type) {
    case "flashcard": {
      const { frontImage, backImage } = card.payload;
      return [frontImage?.mediaId, backImage?.mediaId].filter(
        (mediaId): mediaId is string => Boolean(mediaId),
      );
    }
  }
}

/** Страница редактора набора. */
export function EditSetPage() {
  const { id } = useParams();
  if (!id) return null;
  return (
    <RequireSession>
      <EditSetContent setId={id} />
    </RequireSession>
  );
}

/** Загружает набор и элементы, отдаёт дальше в EditorBoard (монтируется по key=setId). */
function EditSetContent({ setId }: { setId: string }) {
  const { data: set } = useSet(setId);
  const { data: elements, isPending, error } = useElements(setId);

  if (isPending) {
    return (
      <Center h="50vh">
        <Loader />
      </Center>
    );
  }

  if (!elements) {
    const view = toErrorView(error);
    return (
      <Center h="50vh">
        <Stack gap="xs" align="center">
          <Text fw={600}>Не удалось загрузить набор</Text>
          <Text c="dimmed" size="sm">
            {view.message}
          </Text>
        </Stack>
      </Center>
    );
  }

  return (
    <EditorBoard
      key={setId}
      setId={setId}
      title={set?.title}
      initialElements={elements}
    />
  );
}

/** Редактирование: локальный список карточек — источник истины, автозапись debounce+blur. */
function EditorBoard({
  setId,
  title,
  initialElements,
}: {
  setId: string;
  title: string | undefined;
  initialElements: Element[];
}) {
  const [cards, setCards] = useState<EditorCard[]>(() =>
    initialElements.map(toEditorCard),
  );
  const [saveError, setSaveError] = useState<string | null>(null);

  const create = useCreateElement(setId);
  const update = useUpdateElement(setId);
  const remove = useDeleteElement(setId);
  const move = useMoveElement(setId);

  // Один батч-запрос на все картинки набора: ссылки живут 15 минут и продлеваются фоном.
  useMediaLinks(cards.flatMap(cardMediaIds));

  const saveTimers = useRef(new Map<string, number>());
  const creatingDrafts = useRef(new Set<string>());
  const draftCounter = useRef(0);

  const sensors = useSensors(
    // distance 8: drag стартует после сдвига на 8px — клики/скролл не перехватываются.
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    }),
  );

  // Не даём отложенным сохранениям выстрелить после размонтирования (иначе onError сработает на мёртвом компоненте).
  useEffect(() => {
    const timers = saveTimers.current;
    return () => {
      for (const timer of timers.values()) clearTimeout(timer);
    };
  }, []);

  const fail = () =>
    setSaveError("Не удалось сохранить изменения. Проверьте соединение.");

  const saveCard = (card: EditorCard) => {
    update.mutate(
      { elementId: card.id, input: { payload: toApiPayload(card) } },
      { onError: fail },
    );
  };

  const scheduleSave = (card: EditorCard) => {
    if (card.isDraft) return;
    const existing = saveTimers.current.get(card.id);
    if (existing) clearTimeout(existing);
    saveTimers.current.set(
      card.id,
      window.setTimeout(() => {
        saveTimers.current.delete(card.id);
        saveCard(card);
      }, AUTOSAVE_DELAY),
    );
  };

  const flushSave = (id: string) => {
    const existing = saveTimers.current.get(id);
    if (existing) {
      clearTimeout(existing);
      saveTimers.current.delete(id);
    }
    const card = cards.find((c) => c.id === id);
    if (card && !card.isDraft) saveCard(card);
  };

  // Черновик становится элементом, когда payload «полон» (для флеш-карточки — оба поля заполнены).
  const maybeCreate = (card: EditorCard) => {
    if (creatingDrafts.current.has(card.id)) return;
    if (!isComplete(card)) return;

    creatingDrafts.current.add(card.id);
    create.mutate(
      { type: card.type, payload: toApiPayload(card) },
      {
        onSuccess: (element) => {
          creatingDrafts.current.delete(card.id);
          setCards((prev) =>
            prev.map((c) => (c.id === card.id ? toEditorCard(element) : c)),
          );
        },
        onError: () => {
          creatingDrafts.current.delete(card.id);
          fail();
        },
      },
    );
  };

  const changeCard = (updated: EditorCard) => {
    setCards((prev) => prev.map((c) => (c.id === updated.id ? updated : c)));
    if (updated.isDraft) maybeCreate(updated);
    else scheduleSave(updated);
  };

  const addCard = () => {
    const localId = `local-${draftCounter.current++}`;
    setCards((prev) => [
      ...prev,
      {
        id: localId,
        isDraft: true,
        type: "flashcard",
        payload: flashcard.defaultDraft(),
      },
    ]);
  };

  const removeCard = (id: string) => {
    const timer = saveTimers.current.get(id);
    if (timer) {
      clearTimeout(timer);
      saveTimers.current.delete(id);
    }
    const card = cards.find((c) => c.id === id);
    setCards((prev) => prev.filter((c) => c.id !== id));
    if (card && !card.isDraft) remove.mutate(id, { onError: fail });
  };

  const persistOrder = (next: EditorCard[], movedId: string) => {
    setCards(next);

    const moved = next.find((card) => card.id === movedId);
    // Черновик живёт только на клиенте: переставлять на сервере нечего.
    if (!moved || moved.isDraft) return;

    const index = next.findIndex((card) => card.id === movedId);
    const previousId = neighbourId(next, index, -1);
    const nextId = neighbourId(next, index, 1);
    // Единственный элемент в наборе: соседей нет, менять нечего.
    if (!previousId && !nextId) return;

    move.mutate({ elementId: movedId, previousId, nextId }, { onError: fail });
  };

  // Ближайший сохранённый сосед в направлении dir: черновики пропускаются.
  const neighbourId = (list: EditorCard[], index: number, dir: -1 | 1): string | null => {
    for (let i = index + dir; i >= 0 && i < list.length; i += dir) {
      if (!list[i].isDraft) return list[i].id;
    }
    return null;
  };

  const moveCard = (id: string, dir: -1 | 1) => {
    const idx = cards.findIndex((c) => c.id === id);
    const target = idx + dir;
    if (idx < 0 || target < 0 || target >= cards.length) return;
    persistOrder(arrayMove(cards, idx, target), id);
  };

  // DnD: commit нового порядка по факту drop (active -> позиция over).
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;
    const oldIndex = cards.findIndex((c) => c.id === active.id);
    const newIndex = cards.findIndex((c) => c.id === over.id);
    if (oldIndex < 0 || newIndex < 0) return;
    persistOrder(arrayMove(cards, oldIndex, newIndex), String(active.id));
  };

  return (
    <Stack gap="md">
      <Group justify="space-between" align="center">
        <Title order={2}>{title ?? "Набор"}</Title>
        <Group gap="xs" align="center">
          {move.isPending && <Loader size="xs" />}
          <Button
            variant="default"
            component={Link}
            to="/sets"
            leftSection={<ArrowLeft size={16} />}
          >
            Назад
          </Button>
        </Group>
      </Group>

      {saveError && (
        <Alert color="red" withCloseButton onClose={() => setSaveError(null)}>
          {saveError}
        </Alert>
      )}

      {cards.length === 0 ? (
        <Center py="xl">
          <Stack gap="xs" align="center">
            <Text c="dimmed">Набор пуст.</Text>
            <Text c="dimmed" size="sm">
              Добавьте первую карточку.
            </Text>
          </Stack>
        </Center>
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <div
            style={move.isPending ? { pointerEvents: "none", opacity: 0.6 } : undefined}
          >
          <SortableContext
            items={cards.map((c) => c.id)}
            strategy={verticalListSortingStrategy}
          >
            {cards.map((card, index) => (
              <ElementCard
                key={card.id}
                card={card}
                index={index}
                total={cards.length}
                onCardChange={changeCard}
                onBlur={() => flushSave(card.id)}
                onDelete={() => removeCard(card.id)}
                onMoveUp={() => moveCard(card.id, -1)}
                onMoveDown={() => moveCard(card.id, 1)}
              />
            ))}
          </SortableContext>
          </div>
        </DndContext>
      )}

      <Button
        variant="light"
        leftSection={<Plus size={16} />}
        onClick={addCard}
      >
        Добавить карточку
      </Button>
    </Stack>
  );
}
