import {
  Button,
  Group,
  Modal,
  Stack,
  Text,
  Textarea,
  TextInput,
} from "@mantine/core";
import { useState } from "react";

import { toErrorView } from "@/shared/api/errors/errorView";
import {
  useCreateSet,
  useUpdateSet,
} from "@/features/sets/api/useSetMutations";
import type { SetSummary } from "@/features/sets/model/types";

/** Лимиты полей - зеркало домена Sets (SetConstraints), для валидации до отправки. */
const TITLE_MAX = 120;
const DESCRIPTION_MAX = 2000;

const titleError = (value: string): string[] | undefined => {
  const trimmed = value.trim();
  if (!trimmed) return ["Укажите название набора."];
  if (trimmed.length > TITLE_MAX)
    return [`Название - не длиннее ${TITLE_MAX} символов.`];
  return undefined;
};

const descriptionError = (value: string): string[] | undefined =>
  value.trim().length > DESCRIPTION_MAX
    ? [`Описание - не длиннее ${DESCRIPTION_MAX} символов.`]
    : undefined;

interface SetFormModalProps {
  /** null - создание, иначе - редактирование существующего набора. */
  set: SetSummary | null;
  onClose: () => void;
}

/**
 * Модалка создания/редактирования набора. Монтируется только когда открыта:
 * состояние формы и мутации стартуют с чистого листа при каждом открытии.
 */
export function SetFormModal({ set, onClose }: SetFormModalProps) {
  const create = useCreateSet();
  const update = useUpdateSet();

  const [title, setTitle] = useState(set?.title ?? "");
  const [description, setDescription] = useState(set?.description ?? "");
  const [fieldErrors, setFieldErrors] = useState<{
    title?: string[];
    description?: string[];
  }>({});

  const mutationError = create.error ?? update.error;
  const serverView = mutationError ? toErrorView(mutationError) : null;
  const isPending = create.isPending || update.isPending;

  // Ошибка показывается только после попытки отправки. Дальше пересчитываем её на каждый ввод:
  // поле стало валидным - ошибка (и красный цвет) уходит; осталось невалидным - текст обновляется.
  const changeTitle = (value: string) => {
    setTitle(value);
    setFieldErrors((prev) => {
      if (!prev.title) return prev;
      const error = titleError(value);
      const next = { ...prev };
      if (error) next.title = error;
      else delete next.title;
      return next;
    });
  };

  const changeDescription = (value: string) => {
    setDescription(value);
    setFieldErrors((prev) => {
      if (!prev.description) return prev;
      const error = descriptionError(value);
      const next = { ...prev };
      if (error) next.description = error;
      else delete next.description;
      return next;
    });
  };

  const submit = () => {
    const titleErr = titleError(title);
    const descriptionErr = descriptionError(description);
    if (titleErr || descriptionErr) {
      setFieldErrors({
        ...(titleErr ? { title: titleErr } : {}),
        ...(descriptionErr ? { description: descriptionErr } : {}),
      });
      return;
    }
    setFieldErrors({});
    const input = {
      title: title.trim(),
      description: description.trim() || null,
    };
    if (set) update.mutate({ id: set.id, input }, { onSuccess: onClose });
    else create.mutate(input, { onSuccess: onClose });
  };

  return (
    <Modal
      opened
      onClose={onClose}
      title={set ? "Редактировать набор" : "Новый набор"}
      centered
    >
      <Stack gap="md">
        {serverView && (
          <Text c="red" size="sm">
            {serverView.message}
          </Text>
        )}
        <TextInput
          label="Название"
          value={title}
          onChange={(event) => changeTitle(event.currentTarget.value)}
          error={fieldErrors.title?.[0]}
          maxLength={TITLE_MAX + 1}
          required
          data-autofocus
        />
        <Textarea
          label="Описание"
          value={description}
          onChange={(event) => changeDescription(event.currentTarget.value)}
          error={fieldErrors.description?.[0]}
          minRows={3}
          autosize
        />
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>
            Отмена
          </Button>
          <Button loading={isPending} onClick={submit}>
            {set ? "Сохранить" : "Создать"}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}
