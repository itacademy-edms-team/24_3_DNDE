import { apiClient } from "@/shared/api/clients/apiClient";
import type {
  InitUploadInput,
  InitUploadResponse,
  MediaLinksResponse,
  MediaResponse,
} from "@/features/media/model/types";

/** Инициализация загрузки (POST /api/media/uploads, Idempotency-Key обязателен). */
export async function initUpload(
  input: InitUploadInput,
): Promise<InitUploadResponse> {
  const { data } = await apiClient.post<InitUploadResponse>(
    "/media/uploads",
    input,
    {
      headers: { "Idempotency-Key": crypto.randomUUID() },
    },
  );
  return data;
}

/** Подтверждение загрузки (POST /api/media/uploads/{id}/confirm, Idempotency-Key обязателен). */
export async function confirmUpload(mediaId: string): Promise<MediaResponse> {
  const { data } = await apiClient.post<MediaResponse>(
    `/media/uploads/${mediaId}/confirm`,
    undefined,
    { headers: { "Idempotency-Key": crypto.randomUUID() } },
  );
  return data;
}

/** Медиа-объект по id (GET /api/media/{id}). */
export async function getMedia(
  mediaId: string,
  signal?: AbortSignal,
): Promise<MediaResponse> {
  const { data } = await apiClient.get<MediaResponse>(`/media/${mediaId}`, {
    signal,
  });
  return data;
}

/**
 * Ссылки на чтение пачкой (POST /api/media/links): один запрос на все картинки страницы.
 * Возвращает только свои и готовые файлы — остальные обновляются как заглушки.
 */
export async function fetchMediaLinks(
  mediaIds: readonly string[],
  signal?: AbortSignal,
): Promise<MediaLinksResponse> {
  const { data } = await apiClient.post<MediaLinksResponse>(
    "/media/links",
    { mediaIds },
    { signal },
  );
  return data;
}

/**
 * Прямая загрузка файла в S3 (не через API): FormData из подписанных `fields` + файл полем "file".
 * Прогресс — через XHR (fetch не поддерживает upload.onprogress).
 * Успех = любой 2xx (cloud.ru отдаёт 200 даже при success_action_status=201).
 */
export async function uploadToS3(
  url: string,
  fields: Record<string, string>,
  file: File,
  onProgress?: (progress: number) => void,
): Promise<void> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", url);

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable && onProgress)
        onProgress(event.loaded / event.total);
    };
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) resolve();
      else reject(new Error(`Upload failed: HTTP ${xhr.status}`));
    };
    xhr.onerror = () => reject(new Error("Upload failed: network error"));
    xhr.onabort = () => reject(new Error("Upload aborted"));

    const form = new FormData();
    for (const [key, value] of Object.entries(fields)) form.append(key, value);
    form.append("file", file);

    xhr.send(form);
  });
}
