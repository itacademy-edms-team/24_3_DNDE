import { apiClient } from "../../../shared/api/clients/apiClient";
import type { UserProfile } from "../model/types";

/** Профиль текущего пользователя из домена (GET /api/users/me). */
export async function getMyAccount(signal?: AbortSignal): Promise<UserProfile> {
  const { data } = await apiClient.get<UserProfile>("/users/me", { signal });
  return data;
}
