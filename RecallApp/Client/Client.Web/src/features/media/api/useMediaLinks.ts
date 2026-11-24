import { useCallback, useMemo } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";

import { fetchMediaLinks, getMedia } from "./media";
import { mediaKeys } from "./mediaKeys";
import type { MediaFileLinks } from "@/features/media/model/types";

/** Ссылка живёт 15 минут на сервере: переподписываем заранее, чтобы картинка не успела сломаться. */
const REFRESH_BEFORE_MS = 10 * 60 * 1000;

/** Период фоновой переподписи ссылок страницы. */
const REFRESH_AFTER_MS = 12 * 60 * 1000;

async function loadMediaLink(mediaId: string): Promise<MediaFileLinks> {
  const media = await getMedia(mediaId);
  return { variants: media.variants };
}

/**
 * Переподписывает ссылки всех файлов страницы одним запросом и раскладывает их по кэшу файлов,
 * откуда их читают компоненты. Обновляется по таймеру и при возврате к окну (после сна ноутбука).
 */
export function useMediaLinks(mediaIds: readonly string[]) {
  const queryClient = useQueryClient();

  // Массив mediaIds создаётся на каждом рендере, поэтому ключ строим из строки.
  const idsKey = useMemo(
    () => [...new Set(mediaIds)].sort().join(","),
    [mediaIds],
  );
  const ids = useMemo(() => (idsKey === "" ? [] : idsKey.split(",")), [idsKey]);

  return useQuery({
    queryKey: mediaKeys.links(ids),
    queryFn: async ({ signal }) => {
      const response = await fetchMediaLinks(ids, signal);
      for (const [mediaId, file] of Object.entries(response.links))
        queryClient.setQueryData(mediaKeys.link(mediaId), file);
      return response;
    },
    enabled: ids.length > 0,
    staleTime: REFRESH_BEFORE_MS,
    refetchInterval: REFRESH_AFTER_MS,
    refetchOnWindowFocus: true,
  });
}

/**
 * Свежие ссылки одного файла из общего кэша плюс принудительное обновление.
 * Сам по себе не запрашивает: наполняется батчем, `refresh` нужен как фолбэк на ошибке загрузки картинки.
 */
export function useMediaLink(mediaId: string) {
  const queryClient = useQueryClient();

  const { data } = useQuery({
    queryKey: mediaKeys.link(mediaId),
    queryFn: () => loadMediaLink(mediaId),
    enabled: false,
    staleTime: Infinity,
  });

  const refresh = useCallback(async () => {
    if (!mediaId) return undefined;
    const fresh = await queryClient.fetchQuery({
      queryKey: mediaKeys.link(mediaId),
      queryFn: () => loadMediaLink(mediaId),
      staleTime: 0,
    });
    queryClient.setQueryData(mediaKeys.link(mediaId), fresh);
    return fresh;
  }, [mediaId, queryClient]);

  return { variants: data?.variants, refresh };
}
