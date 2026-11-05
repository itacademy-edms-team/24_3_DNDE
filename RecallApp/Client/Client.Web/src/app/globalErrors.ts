import { canRedirectToLogin, markLoginRedirect } from "@/features/session/model/authRedirectGuard";
import { loginUrl } from "@/features/session/model/sessionUrls";
import { toErrorView } from "@/shared/api/errors/errorView";
import { router } from "./router";

/** Защита от цикла: один раз инициированный переход на вход не повторяем. */
let redirecting = false;

/**
 * Глобальная обработка ошибок запросов и мутаций (QueryCache/MutationCache).
 * Сейчас единственная сквозная реакция - утрата сессии: уводим на вход.
 * Остальные ошибки показывают компоненты через toErrorView.
 */
export function handleGlobalError(error: unknown): void {
  const view = toErrorView(error);
  if (!view.unauthenticated || redirecting) return;

  // Бюджет по редиректам в bff исчерпан.
  // Если после возврата с входа сессия так и не появилась, то ничего не делаем.
  // Ещё один редирект запустил бы бесконечный цикл spa -> bff -> spa.
  if (!canRedirectToLogin()) return;

  redirecting = true;
  markLoginRedirect();
  const location = router.state.location;
  window.location.assign(loginUrl(`${location.pathname}${location.search}`));
}
