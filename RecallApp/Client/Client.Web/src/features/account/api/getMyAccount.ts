import { apiFetch } from "../../../shared/api/http";
import type { UserProfile } from "../model/types";

/** Профиль текущего пользователя из домена (GET /api/users/me). */
export async function getMyAccount(signal?: AbortSignal): Promise<UserProfile> {
  return apiFetch<UserProfile>("/users/me", { signal });
}
