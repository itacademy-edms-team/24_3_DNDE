import { useEffect, useRef, useState } from "react";
import { Alert, Button, Center, Group, Loader, Stack, Text, Title } from "@mantine/core";
import { ArrowLeft, Plus } from "@phosphor-icons/react";
import { Link, useParams } from "react-router";

import { toErrorView } from "@/shared/api/errors/errorView";
import { RequireSession } from "@/features/session/ui/RequireSession";
import { useSet } from "@/features/sets/api/useSet";
import { useElements } from "@/features/sets/api/useElements";
import {
  useCreateElement,
  useDeleteElement,
  useReorderElements,
  useUpdateElement,
} from "@/features/sets/api/useElementMutations";
import { ElementCard, type EditorCard } from "@/features/sets/ui/ElementCard";
import type { Element } from "@/features/sets/model/types";

const AUTOSAVE_DELAY = 600;

const toEditorCard = (element: Element): EditorCard => ({
  id: element.id,
  isDraft: false,
  term: element.payload.term,
  definition: element.payload.definition,
  frontImageId: element.payload.frontImageId,
  backImageId: element.payload.backImageId,
});

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

  return <EditorBoard key={setId} setId={setId} title={set?.title} initialElements={elements} />;
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
  const [cards, setCards] = useState<EditorCard[]>(() => initialElements.map(toEditorCard));
  const [saveError, setSaveError] = useState<string | null>(null);

  const create = useCreateElement(setId);
  const update = useUpdateElement(setId);
  const remove = useDeleteElement(setId);
  const reorder = useReorderElements(setId);

  const saveTimers = useRef(new Map<string, number>());
  const creatingDrafts = useRef(new Set<string>());
  const draftCounter = useRef(0);

  // Не даём отложенным сохранениям выстрелить после размонтирования (иначе onError сработает на мёртвом компоненте).
  useEffect(() => {
    const timers = saveTimers.current;
    return () => {
      for (const timer of timers.values()) clearTimeout(timer);
    };
  }, []);

  const fail = () => setSaveError("Не удалось сохранить изменения. Проверьте соединение.");

  const saveCard = (card: EditorCard) => {
    update.mutate(
      {
        elementId: card.id,
        input: {
          payload: {
            term: card.term,
            definition: card.definition,
            frontImageId: card.frontImageId,
            backImageId: card.backImageId,
          },
        },
      },
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

  // Черновик становится элементом, когда оба поля заполнены (бэкенд требует непустые term/definition).
  const maybeCreate = (id: string, term: string, definition: string) => {
    if (creatingDrafts.current.has(id)) return;
    if (!term.trim() || !definition.trim()) return;

    creatingDrafts.current.add(id);
    create.mutate(
      { type: "flashcard", payload: { term: term.trim(), definition: definition.trim() } },
      {
        onSuccess: (element) => {
          creatingDrafts.current.delete(id);
          setCards((prev) =>
            prev.map((c) =>
              c.id === id
                ? {
                    id: element.id,
                    isDraft: false,
                    term: element.payload.term,
                    definition: element.payload.definition,
                    frontImageId: element.payload.frontImageId,
                    backImageId: element.payload.backImageId,
                  }
                : c,
            ),
          );
        },
        onError: () => {
          creatingDrafts.current.delete(id);
          fail();
        },
      },
    );
  };

  const changeTerm = (card: EditorCard, value: string) => {
    setCards((prev) => prev.map((c) => (c.id === card.id ? { ...c, term: value } : c)));
    if (card.isDraft) maybeCreate(card.id, value, card.definition);
    else scheduleSave({ ...card, term: value });
  };

  const changeDefinition = (card: EditorCard, value: string) => {
    setCards((prev) => prev.map((c) => (c.id === card.id ? { ...c, definition: value } : c)));
    if (card.isDraft) maybeCreate(card.id, card.term, value);
    else scheduleSave({ ...card, definition: value });
  };

  const addCard = () => {
    const localId = `local-${draftCounter.current++}`;
    setCards((prev) => [
      ...prev,
      {
        id: localId,
        isDraft: true,
        term: "",
        definition: "",
        frontImageId: null,
        backImageId: null,
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

  const moveCard = (id: string, dir: -1 | 1) => {
    const idx = cards.findIndex((c) => c.id === id);
    const target = idx + dir;
    if (idx < 0 || target < 0 || target >= cards.length) return;
    if (cards[idx].isDraft) return;

    const next = [...cards];
    [next[idx], next[target]] = [next[target], next[idx]];
    setCards(next);
    reorder.mutate(
      next.filter((c) => !c.isDraft).map((c) => c.id),
      { onError: fail },
    );
  };

  return (
    <Stack gap="md">
      <Group justify="space-between" align="center">
        <Title order={2}>{title ?? "Набор"}</Title>
        <Button variant="default" component={Link} to="/sets" leftSection={<ArrowLeft size={16} />}>
          Назад
        </Button>
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
        cards.map((card, index) => (
          <ElementCard
            key={card.id}
            card={card}
            index={index}
            total={cards.length}
            onTermChange={(value) => changeTerm(card, value)}
            onDefinitionChange={(value) => changeDefinition(card, value)}
            onBlur={() => flushSave(card.id)}
            onDelete={() => removeCard(card.id)}
            onMoveUp={() => moveCard(card.id, -1)}
            onMoveDown={() => moveCard(card.id, 1)}
          />
        ))
      )}

      <Button variant="light" leftSection={<Plus size={16} />} onClick={addCard}>
        Добавить карточку
      </Button>
    </Stack>
  );
}
