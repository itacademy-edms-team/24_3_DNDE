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
 * Тело POST /api/sets. `title: null` допустим - домен подставит имя по умолчанию
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
