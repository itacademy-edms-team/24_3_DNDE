import { apiClient } from "@/shared/api/clients/apiClient";

/** Удаление набора (DELETE /api/sets/{id}): мягкое удаление, 204 при успехе. */
export async function deleteSet(id: string): Promise<void> {
  await apiClient.delete(`/sets/${id}`);
}
