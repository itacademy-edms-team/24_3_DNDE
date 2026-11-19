import { apiClient } from "@/shared/api/clients/apiClient";
import type {
  CreateElementInput,
  Element,
  ElementsPage,
  UpdateElementInput,
} from "@/features/sets/model/types";

/** Страница элементов набора (GET /api/sets/{setId}/elements, keyset-пагинация). */
export async function listElements(
  setId: string,
  cursor: string | null,
  pageSize: number,
  signal?: AbortSignal,
): Promise<ElementsPage> {
  const { data } = await apiClient.get<ElementsPage>(
    `/sets/${setId}/elements`,
    {
      params: { cursor: cursor ?? undefined, pageSize },
      signal,
    },
  );
  return data;
}

/**
 * Все элементы набора — прокручивает курсор до конца.
 * Редактору нужен полный список: перестановка считается по соседям из него.
 */
export async function listAllElements(
  setId: string,
  signal?: AbortSignal,
): Promise<Element[]> {
  const items: Element[] = [];
  let cursor: string | null = null;
  do {
    const page = await listElements(setId, cursor, 100, signal);
    items.push(...page.items);
    cursor = page.nextCursor;
  } while (cursor);
  return items;
}

/** Создание элемента (POST .../elements, Idempotency-Key обязателен). */
export async function createElement(
  setId: string,
  input: CreateElementInput,
): Promise<Element> {
  const { data } = await apiClient.post<Element>(
    `/sets/${setId}/elements`,
    input,
    {
      headers: { "Idempotency-Key": crypto.randomUUID() },
    },
  );
  return data;
}

/** Обновление содержимого элемента (PUT .../elements/{elementId}: только payload). */
export async function updateElement(
  setId: string,
  elementId: string,
  input: UpdateElementInput,
): Promise<Element> {
  const { data } = await apiClient.put<Element>(
    `/sets/${setId}/elements/${elementId}`,
    input,
  );
  return data;
}

/** Удаление элемента (DELETE .../elements/{elementId}). */
export async function deleteElement(
  setId: string,
  elementId: string,
): Promise<void> {
  await apiClient.delete(`/sets/${setId}/elements/${elementId}`);
}

/**
 * Перестановка элемента по соседям (POST .../elements/{elementId}/move).
 * Соседи определяются на клиенте после drag&drop: элемент встаёт между ними.
 */
export async function moveElement(
  setId: string,
  elementId: string,
  previousId: string | null,
  nextId: string | null,
): Promise<Element> {
  const { data } = await apiClient.post<Element>(
    `/sets/${setId}/elements/${elementId}/move`,
    { previousId, nextId },
  );
  return data;
}
