/** Ключи кэша TanStack Query для элементов набора. */
export const elementsKeys = {
  list: (setId: string) => ["sets", "elements", "list", setId] as const,
};
