/**
 * Набор в списке «Мои наборы» (контракт GET /api/sets).
 * POST /api/sets и PUT /api/sets/{id} возвращают те же поля.
 */
export interface SetSummary {
  id: string;
  title: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

/** Страница списка наборов: элементы и курсор следующей страницы (keyset, ADR 0039). */
export interface SetsPage {
  items: SetSummary[];
  nextCursor: string | null;
}

/**
 * Тело POST /api/sets. `title: null` допустим — домен подставит имя по умолчанию
 * (используется быстрой карточкой, US-5). В форме создания название обязательно.
 */
export interface CreateSetInput {
  title: string | null;
  description: string | null;
}

/** Тело PUT /api/sets/{id}: полная замена названия и описания. */
export interface UpdateSetInput {
  title: string | null;
  description: string | null;
}

// ===== Элементы =====

/** Ссылка на вариант изображения (MediaImageLink): подписанный URL и размеры. */
export interface ElementImageLink {
  url: string;
  width: number;
  height: number;
}

/** Сторона карточки: варианты thumb/display. */
export interface ElementSideImages {
  thumb: ElementImageLink;
  display: ElementImageLink;
}

/** Изображения элемента: сторона -> варианты. null — изображений нет. */
export interface ElementImages {
  front?: ElementSideImages;
  back?: ElementSideImages;
}

/** Известные типы элементов. Новый тип = добавить значение + вариант в Element/ElementPayloadInput. */
export type ElementType = "flashcard";

/** Общие поля элемента, одинаковые для всех типов. */
interface ElementBase {
  id: string;
  images: ElementImages | null;
  position: number;
  createdAt: string;
  updatedAt: string;
}

/** Payload флеш-карточки в ответе API (frontImageId/backImageId всегда присутствуют). */
export interface FlashcardPayload {
  term: string;
  definition: string;
  frontImageId: string | null;
  backImageId: string | null;
}

/** Элемент-флеш-карточка. */
export interface FlashcardElement extends ElementBase {
  type: "flashcard";
  payload: FlashcardPayload;
}

/**
 * Элемент набора в ответе API — размеченное объединение по `type`.
 * Потребитель сужает по `element.type`; новый тип = новый вариант + ветки в dispatch-точках.
 */
export type Element = FlashcardElement;

/** Страница элементов (keyset-пагинация). */
export interface ElementsPage {
  items: Element[];
  nextCursor: string | null;
}

/** Payload флеш-карточки в теле запроса: изображения необязательны. */
export interface FlashcardPayloadInput {
  term: string;
  definition: string;
  frontImageId?: string | null;
  backImageId?: string | null;
}

/** Payload в теле запроса — объединение по типам элементов. */
export type ElementPayloadInput = FlashcardPayloadInput;

/** Тело POST /api/sets/{setId}/elements. */
export interface CreateElementInput {
  type: ElementType;
  payload: ElementPayloadInput;
  position?: number | null;
}

/** Тело PUT /api/sets/{setId}/elements/{elementId}: тип неизменяем, только payload. */
export interface UpdateElementInput {
  payload: ElementPayloadInput;
}
