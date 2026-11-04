/**
 * Коды ошибок бэкенда, для которых есть текст на человеческом языке.
 * Ключ - problem.title (домен Accounts - PascalCase, отказы BFF - snake_case).
 */
const restCodeMessages: Record<string, string> = {
  // Домен Accounts
  InvalidKeycloakSub: "Не удалось определить пользователя. Войдите заново.",
  InvalidEmail: "Некорректный email.",
  InvalidDisplayName: "Недопустимое отображаемое имя.",
  // BFF
  not_authenticated: "Требуется вход в аккаунт.",
  session_expired: "Сессия истекла. Войдите заново.",
  csrf_failed: "Запрос отклонён проверкой безопасности. Обновите страницу и повторите.",
};

/** Возвращает текст для известного кода ошибки; null - код неизвестен, нужен fallback. */
export function restCodeMessage(code: string | undefined): string | null {
  if (!code) return null;
  return restCodeMessages[code] ?? null;
}
