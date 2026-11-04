const StorageKey = "recallapp.authRedirectCount";

/**
 * Сколько раз подряд можно уйти на вход, не получив сессию.
 * Больше указанного количества - значит возврат с BFF не даёт сессии (cookie не передаётся, CORS, битый returnUrl),
 * и повторный переход превращается в бесконечную петлю из редиректов.
 */
const MaxRedirects = 1;

/** Читает кол-во редирект-попыток из sessionStorage. Если хранилище не доступно, возвращает 0 */
function readAttempts(): number {
  try {
    return Number(sessionStorage.getItem(StorageKey) ?? "0");
  } catch {
    // Недоступное хранилище (приватный режим) не должно блокировать вход.
    return 0;
  }
}

/** Проверяет, остались ли ещё попытки на повторный редирект. */
export function canRedirectToLogin(): boolean {
  return readAttempts() < MaxRedirects;
}

/**
 * Считает попытку перехода на вход.
 * Счётчик живёт в sessionStorage: он переживает полную перезагрузку страницы (именно так
 * выглядит возврат с BFF), но не переживает закрытие вкладки.
 */
export function markLoginRedirect(): void {
  try {
    sessionStorage.setItem(StorageKey, String(readAttempts() + 1));
  } catch {
    // Если счётчик недоступен, разрешить редирект.
  }
}

/** Сбрасывает счётчик после успешно установленной сессии. */
export function resetAuthRedirects(): void {
  try {
    sessionStorage.removeItem(StorageKey);
  } catch {
    // Игнорируем: сбрасывать нечего.
  }
}
