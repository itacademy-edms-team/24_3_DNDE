import type {
  ElementImages,
  ElementSideImages,
  FlashcardPayload,
  FlashcardPayloadInput,
} from "@/features/sets/model/types";

/** Изображение стороны карточки: id файла + превью (thumb) для отображения в редакторе. */
export interface FlashcardSideImage {
  mediaId: string;
  thumbUrl: string;
}

/** Черновик флеш-карточки в редакторе (текст + изображения сторон). */
export interface FlashcardDraft {
  term: string;
  definition: string;
  frontImage: FlashcardSideImage | null;
  backImage: FlashcardSideImage | null;
}

function toSideImage(
  mediaId: string | null,
  side: ElementSideImages | undefined,
): FlashcardSideImage | null {
  if (!mediaId || !side?.thumb) return null;
  return { mediaId, thumbUrl: side.thumb.url };
}

/**
 * Всё специфичное для флеш-карточки: черновик, преобразования в/из payload,
 * признак «полноты» (готовности к сохранению).
 * Новый тип элемента получает свой модуль с тем же набором экспортов.
 */
export const flashcard = {
  defaultDraft(): FlashcardDraft {
    return { term: "", definition: "", frontImage: null, backImage: null };
  },

  toDraft(
    payload: FlashcardPayload,
    images: ElementImages | null,
  ): FlashcardDraft {
    return {
      term: payload.term,
      definition: payload.definition,
      frontImage: toSideImage(payload.frontImageId, images?.front),
      backImage: toSideImage(payload.backImageId, images?.back),
    };
  },

  toApiPayload(draft: FlashcardDraft): FlashcardPayloadInput {
    return {
      term: draft.term,
      definition: draft.definition,
      frontImageId: draft.frontImage?.mediaId ?? null,
      backImageId: draft.backImage?.mediaId ?? null,
    };
  },

  // Карточка «полна», когда заполнены оба текстовых поля (бэкенд требует непустые term/definition).
  isComplete(draft: FlashcardDraft): boolean {
    return draft.term.trim() !== "" && draft.definition.trim() !== "";
  },
};
