import {
  Button,
  Group,
  Modal,
  Stack,
  Text,
  Textarea,
  TextInput,
} from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router";

import { toErrorView } from "@/shared/api/errors/errorView";
import { createSet } from "@/features/sets/api/createSet";
import { createElement } from "@/features/sets/api/elements";
import { setsKeys } from "@/features/sets/api/setsKeys";

/** Лимиты — зеркало ElementConstraints (домен Sets), для валидации до отправки. */
const TERM_MAX = 500;
const DEFINITION_MAX = 2000;

/** Дефолтное имя быстрой карточки: бэкенд не подставляет его сам (требует непустой title). */
const defaultSetTitle = () =>
  `Мой набор от ${new Date().toLocaleDateString("ru-RU")}`;

const termError = (value: string): string[] | undefined => {
  const trimmed = value.trim();
  if (!trimmed) return ["Укажите термин."];
  if (trimmed.length > TERM_MAX)
    return [`Термин — не длиннее ${TERM_MAX} символов.`];
  return undefined;
};

const definitionError = (value: string): string[] | undefined => {
  const trimmed = value.trim();
  if (!trimmed) return ["Укажите определение."];
  if (trimmed.length > DEFINITION_MAX)
    return [`Определение — не длиннее ${DEFINITION_MAX} символов.`];
  return undefined;
};

interface QuickCardModalProps {
  onClose: () => void;
}

/**
 * Быстрая карточка (US-5): термин + определение → набор с дефолтным именем + первый элемент,
 * затем переход в редактор. Монтируется только когда открыта — чистое состояние на каждый вход.
 */
export function QuickCardModal({ onClose }: QuickCardModalProps) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [term, setTerm] = useState("");
  const [definition, setDefinition] = useState("");
  const [fieldErrors, setFieldErrors] = useState<{
    term?: string[];
    definition?: string[];
  }>({});

  const quickCreate = useMutation({
    mutationFn: async (input: { term: string; definition: string }) => {
      // Бэкенд требует непустой title — дефолтное имя генерирует клиент.
      const set = await createSet({
        title: defaultSetTitle(),
        description: null,
      });
      await createElement(set.id, { type: "flashcard", payload: input });
      return set.id;
    },
    onSuccess: (setId) => {
      queryClient.invalidateQueries({ queryKey: setsKeys.list() });
      onClose();
      navigate(`/sets/${setId}/edit`);
    },
  });

  const serverView = quickCreate.error ? toErrorView(quickCreate.error) : null;

  // Ошибка показывается после попытки отправки; дальше пересчитываем её на каждый ввод,
  // чтобы красный цвет уходил, как только поле стало валидным.
  const changeTerm = (value: string) => {
    setTerm(value);
    setFieldErrors((prev) => {
      if (!prev.term) return prev;
      const error = termError(value);
      const next = { ...prev };
      if (error) next.term = error;
      else delete next.term;
      return next;
    });
  };

  const changeDefinition = (value: string) => {
    setDefinition(value);
    setFieldErrors((prev) => {
      if (!prev.definition) return prev;
      const error = definitionError(value);
      const next = { ...prev };
      if (error) next.definition = error;
      else delete next.definition;
      return next;
    });
  };

  const submit = () => {
    const termErr = termError(term);
    const definitionErr = definitionError(definition);
    if (termErr || definitionErr) {
      setFieldErrors({
        ...(termErr ? { term: termErr } : {}),
        ...(definitionErr ? { definition: definitionErr } : {}),
      });
      return;
    }
    setFieldErrors({});
    quickCreate.mutate({ term: term.trim(), definition: definition.trim() });
  };

  return (
    <Modal opened onClose={onClose} title="Быстрая карточка" centered>
      <Stack gap="md">
        <Text size="sm" c="dimmed">
          Создаст набор с одной карточкой. Название набора подставится
          автоматически.
        </Text>
        {serverView && (
          <Text c="red" size="sm">
            {serverView.message}
          </Text>
        )}
        <TextInput
          label="Термин"
          value={term}
          onChange={(event) => changeTerm(event.currentTarget.value)}
          error={fieldErrors.term?.[0]}
          maxLength={TERM_MAX + 1}
          required
          data-autofocus
        />
        <Textarea
          label="Определение"
          value={definition}
          onChange={(event) => changeDefinition(event.currentTarget.value)}
          error={fieldErrors.definition?.[0]}
          minRows={3}
          autosize
        />
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>
            Отмена
          </Button>
          <Button loading={quickCreate.isPending} onClick={submit}>
            Создать
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}
