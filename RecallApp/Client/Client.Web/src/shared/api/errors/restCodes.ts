/**
 * Коды ошибок бэкенда, для которых есть текст на человеческом языке.
 * Ключ - problem.title.
 * Происхождение ошибки приходит отдельно, в problem.source.
 */
export const restCodeMessages: Record<string, string> = {
  // Домен Accounts
  InvalidKeycloakSub: "Не удалось определить пользователя. Войдите заново.",
  InvalidEmail: "Некорректный email.",
  InvalidDisplayName: "Недопустимое отображаемое имя.",
  // BFF
  NotAuthenticated: "Требуется вход в аккаунт.",
  SessionExpired: "Сессия истекла. Войдите заново.",
  CsrfFailed: "Запрос отклонён проверкой безопасности. Обновите страницу и повторите.",
};

/** Возвращает текст для известного кода ошибки; null - код неизвестен, нужен fallback. */
export function restCodeMessage(code: string | undefined): string | null {
  if (!code) return null;
  return restCodeMessages[code] ?? null;
}
