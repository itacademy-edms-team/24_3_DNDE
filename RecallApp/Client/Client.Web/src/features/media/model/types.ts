/** Статус медиа-объекта. */
export type MediaStatus = "pending" | "ready" | "failed" | "deleted";

/** Вариант изображения (thumb/display): подписанный URL и размеры. */
export interface MediaVariant {
  url: string;
  width: number;
  height: number;
  sizeBytes: number;
}

/** Ответ confirm/GET /api/media/{id}: состояние + подписанные варианты. */
export interface MediaResponse {
  mediaId: string;
  status: MediaStatus;
  failureCode: string | null;
  createdAt: string;
  variants: {
    thumb?: MediaVariant;
    display?: MediaVariant;
  };
}

/** Тело POST /api/media/uploads. */
export interface InitUploadInput {
  contentType: string;
  sizeBytes: number;
}

/** Ответ инициализации: куда и с какими полями отправить файл напрямую в хранилище. */
export interface InitUploadResponse {
  mediaId: string;
  url: string;
  fields: Record<string, string>;
  objectKey: string;
  expiresInSeconds: number;
}

/** Ссылка на готовый вариант: адрес для чтения и фактические размеры. */
export interface MediaImageLink {
  url: string;
  width: number;
  height: number;
}

/** Ссылки на варианты одного файла: клиент выбирает размер сам. */
export interface MediaFileLinks {
  variants: {
    thumb?: MediaImageLink;
    display?: MediaImageLink;
  };
}

/** Ответ POST /api/media/links: карта «mediaId → варианты». Чужих и неготовых файлов в ней нет. */
export interface MediaLinksResponse {
  links: Record<string, MediaFileLinks>;
}

/** Максимальный размер файла (10 МБ) — зеркало MediaConstraints.MaxUploadBytes. */
export const MEDIA_MAX_BYTES = 10 * 1024 * 1024;

/** Белый список типов (зеркало MediaContentTypes). */
export const MEDIA_CONTENT_TYPES = [
  "image/png",
  "image/jpeg",
  "image/webp",
] as const;
