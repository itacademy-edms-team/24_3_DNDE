import { apiClient } from "@/shared/api/clients/apiClient";
import type { SetSummary } from "@/features/sets/model/types";

/** Набор по id (GET /api/sets/{id}), без элементов. */
export async function getSet(id: string, signal?: AbortSignal): Promise<SetSummary> {
  const { data } = await apiClient.get<SetSummary>(`/sets/${id}`, { signal });
  return data;
}
