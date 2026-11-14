import { apiClient } from "@/shared/api/clients/apiClient";
import type { SetsPage } from "@/features/sets/model/types";

/**
 * Список наборов текущего пользователя (GET /api/sets).
 * Пагинация keyset: следующая страница запрашивается курсором из nextCursor.
 */
export async function listSets(cursor: string | null, signal?: AbortSignal): Promise<SetsPage> {
  const { data } = await apiClient.get<SetsPage>("/sets", {
    params: cursor ? { cursor } : undefined,
    signal,
  });
  return data;
}
