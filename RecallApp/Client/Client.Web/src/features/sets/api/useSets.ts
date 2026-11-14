import { useInfiniteQuery } from "@tanstack/react-query";

import { useSession } from "@/features/session/api/useSession";
import { listSets } from "./listSets";
import { setsKeys } from "./setsKeys";

/** Бесконечный список наборов: следующая страница догружается курсором из nextCursor. */
export function useSets() {
  const { data: session } = useSession();

  return useInfiniteQuery({
    queryKey: setsKeys.list(),
    queryFn: ({ pageParam, signal }) => listSets(pageParam, signal),
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) => lastPage.nextCursor,
    enabled: session?.isAuthenticated === true,
  });
}
