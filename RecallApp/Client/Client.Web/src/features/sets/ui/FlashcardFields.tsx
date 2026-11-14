import { Divider, Text, Textarea, TextInput } from "@mantine/core";

import type { FlashcardDraft } from "./flashcard";

/** Поля флеш-карточки: лицевая (термин) / оборотная (определение). Без логики сохранения. */
export function FlashcardFields({
  draft,
  onChange,
  onBlur,
}: {
  draft: FlashcardDraft;
  onChange: (draft: FlashcardDraft) => void;
  onBlur: () => void;
}) {
  return (
    <>
      <Text size="xs" c="dimmed" tt="uppercase" fw={700} mb="xs">
        Лицевая сторона
      </Text>
      <TextInput
        placeholder="Термин"
        value={draft.term}
        onChange={(event) => onChange({ ...draft, term: event.currentTarget.value })}
        onBlur={onBlur}
      />
      <Divider my="md" />
      <Text size="xs" c="dimmed" tt="uppercase" fw={700} mb="xs">
        Оборотная сторона
      </Text>
      <Textarea
        placeholder="Определение"
        value={draft.definition}
        onChange={(event) => onChange({ ...draft, definition: event.currentTarget.value })}
        onBlur={onBlur}
        minRows={3}
        autosize
      />
    </>
  );
}
