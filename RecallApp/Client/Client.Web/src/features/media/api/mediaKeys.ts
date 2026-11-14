/**
 * Ключи кэша ссылок на медиа.
 * `link` — ссылки одного файла (их наполняет батч и читают компоненты),
 * `links` — сам батч-запрос страницы.
 */
export const mediaKeys = {
  all: ["media"] as const,
  link: (mediaId: string) => [...mediaKeys.all, "link", mediaId] as const,
  links: (mediaIds: readonly string[]) =>
    [...mediaKeys.all, "links", [...mediaIds].sort().join(",")] as const,
};
