import { Divider, Text, Textarea, TextInput } from "@mantine/core";

import {
  MediaImageSlot,
  type MediaSlotImage,
} from "@/features/media/ui/MediaImageSlot";
import type { FlashcardDraft } from "./flashcard";

/** Поля флеш-карточки: лицевая (картинка + термин) / оборотная (картинка + определение). */
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
      <MediaImageSlot
        image={draft.frontImage}
        onChange={(image) => onChange({ ...draft, frontImage: image })}
        label="Лицевая сторона"
      />
      <TextInput
        placeholder="Термин"
        value={draft.term}
        onChange={(event) =>
          onChange({ ...draft, term: event.currentTarget.value })
        }
        onBlur={onBlur}
        mt="sm"
      />

      <Divider my="md" />

      <Text size="xs" c="dimmed" tt="uppercase" fw={700} mb="xs">
        Оборотная сторона
      </Text>
      <MediaImageSlot
        image={draft.backImage}
        onChange={(image: MediaSlotImage | null) =>
          onChange({ ...draft, backImage: image })
        }
        label="Оборотная сторона"
      />
      <Textarea
        placeholder="Определение"
        value={draft.definition}
        onChange={(event) =>
          onChange({ ...draft, definition: event.currentTarget.value })
        }
        onBlur={onBlur}
        minRows={3}
        autosize
        mt="sm"
      />
    </>
  );
}
