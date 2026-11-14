/** Ключи кэша TanStack Query для наборов. */
export const setsKeys = {
  list: () => ["sets", "list"] as const,
  detail: (id: string) => ["sets", "detail", id] as const,
};
