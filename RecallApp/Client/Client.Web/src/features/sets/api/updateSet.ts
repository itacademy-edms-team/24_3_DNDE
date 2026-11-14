import { apiClient } from "@/shared/api/clients/apiClient";
import type { SetSummary, UpdateSetInput } from "@/features/sets/model/types";

/** Обновление набора (PUT /api/sets/{id}): полная замена названия и описания. */
export async function updateSet(id: string, input: UpdateSetInput): Promise<SetSummary> {
  const { data } = await apiClient.put<SetSummary>(`/sets/${id}`, input);
  return data;
}
