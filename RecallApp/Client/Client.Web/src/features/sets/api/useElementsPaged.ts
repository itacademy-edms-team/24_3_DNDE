import { useInfiniteQuery } from "@tanstack/react-query";

import { useSession } from "@/features/session/api/useSession";
import { listElements } from "./elements";
import { elementsKeys } from "./elementsKeys";

/** Размер страницы предпросмотра: маленькие порции, чтобы не тянуть весь набор сразу. */
const PAGE_SIZE = 20;

/**
 * Постраничная загрузка элементов (keyset) — для предпросмотра, где весь список сразу не нужен:
 * грузим первую страницу и догружаем следующие по мере листания. Итог берём из SetDetail.elementCount.
 */
export function useElementsPaged(setId: string) {
  const { data: session } = useSession();

  return useInfiniteQuery({
    queryKey: elementsKeys.paged(setId),
    queryFn: ({ pageParam, signal }) =>
      listElements(setId, pageParam, PAGE_SIZE, signal),
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) => lastPage.nextCursor,
    enabled: session?.isAuthenticated === true,
  });
}
