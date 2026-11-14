import { apiClient } from "@/shared/api/clients/apiClient";
import type { SetDetail } from "@/features/sets/model/types";

/** Набор по id (GET /api/sets/{id}): с числом элементов. */
export async function getSet(
  id: string,
  signal?: AbortSignal,
): Promise<SetDetail> {
  const { data } = await apiClient.get<SetDetail>(`/sets/${id}`, { signal });
  return data;
}
