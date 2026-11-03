/** Профиль пользователя в домене Accounts (контракт GET /api/users/me). */
export interface UserProfile {
  id: string;
  displayName: string;
  email: string;
  emailVerified: boolean;
}
