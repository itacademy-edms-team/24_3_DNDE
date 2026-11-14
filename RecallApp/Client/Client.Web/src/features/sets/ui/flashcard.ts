import type { FlashcardPayload, FlashcardPayloadInput } from "@/features/sets/model/types";

/** Черновик флеш-карточки в редакторе (текст + id изображений). */
export interface FlashcardDraft {
  term: string;
  definition: string;
  frontImageId: string | null;
  backImageId: string | null;
}

/**
 * Всё специфичное для флеш-карточки: черновик, преобразования в/из payload,
 * признак «полноты» (готовности к сохранению).
 * Новый тип элемента получает свой модуль с тем же набором экспортов.
 */
export const flashcard = {
  defaultDraft(): FlashcardDraft {
    return { term: "", definition: "", frontImageId: null, backImageId: null };
  },

  toDraft(payload: FlashcardPayload): FlashcardDraft {
    return {
      term: payload.term,
      definition: payload.definition,
      frontImageId: payload.frontImageId,
      backImageId: payload.backImageId,
    };
  },

  toApiPayload(draft: FlashcardDraft): FlashcardPayloadInput {
    return {
      term: draft.term,
      definition: draft.definition,
      frontImageId: draft.frontImageId,
      backImageId: draft.backImageId,
    };
  },

  // Карточка «полна», когда заполнены оба текстовых поля (бэкенд требует непустые term/definition).
  isComplete(draft: FlashcardDraft): boolean {
    return draft.term.trim() !== "" && draft.definition.trim() !== "";
  },
};
