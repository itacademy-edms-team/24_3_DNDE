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
  CsrfFailed:
    "Запрос отклонён проверкой безопасности. Обновите страницу и повторите.",
  // Домен Sets (коды вида <Сущность>.<Правило>, ADR 0038)
  "Set.NotFound": "Набор не найден.",
  "Set.TitleRequired": "Укажите название набора.",
  "Set.TitleTooLong": "Название набора слишком длинное.",
  "Set.DescriptionTooLong": "Описание набора слишком длинное.",
  // Домен Sets — элементы
  "Element.NotFound": "Карточка не найдена.",
  "Element.TermRequired": "Укажите термин.",
  "Element.TermTooLong": "Термин слишком длинный.",
  "Element.DefinitionRequired": "Укажите определение.",
  "Element.DefinitionTooLong": "Определение слишком длинное.",
  "Element.TypeUnsupported": "Тип элемента не поддерживается.",
  "Element.PayloadInvalid": "Неверный формат содержимого карточки.",
  // Домен Media
  "Media.NotFound": "Файл не найден.",
  "Media.NotReady": "Файл ещё обрабатывается.",
  "Media.InUse": "Файл используется и не может быть удалён.",
  "Media.UploadExpired": "Время загрузки истекло. Попробуйте снова.",
  "Media.ObjectMissing": "Файл не был загружен. Попробуйте снова.",
  "Media.ContentTypeUnsupported": "Поддерживаются только PNG, JPEG или WebP.",
  "Media.FileTooLarge": "Файл слишком большой (до 10 МБ).",
  "Media.InvalidFormat": "Не удалось распознать изображение.",
  "Media.UnsupportedAnimation": "Анимированные изображения не поддерживаются.",
  "Media.ResolutionTooLarge": "Слишком большое разрешение изображения.",
  "Media.ImageCorrupt": "Изображение повреждено.",
  "Media.PendingLimitReached":
    "Слишком много незавершённых загрузок. Подождите немного.",
  "Media.UserUnresolved": "Не удалось определить пользователя.",
  // Идемпотентность POST-создания (общий заголовок Idempotency-Key)
  "Idempotency.KeyRequired": "Запрос не прошёл проверку повторной отправки.",
  "Idempotency.KeyInvalid": "Запрос не прошёл проверку повторной отправки.",
  "Idempotency.KeyReused":
    "Этот запрос уже был выполнен с другим содержимым. Обновите страницу.",
};

/** Возвращает текст для известного кода ошибки; null - код неизвестен, нужен fallback. */
export function restCodeMessage(code: string | undefined): string | null {
  if (!code) return null;
  return restCodeMessages[code] ?? null;
}
