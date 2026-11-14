import { useQuery } from "@tanstack/react-query";

import { useSession } from "@/features/session/api/useSession";
import { listAllElements } from "./elements";
import { elementsKeys } from "./elementsKeys";

/** Все элементы набора (для редактора): догружает страницы до конца курсора. */
export function useElements(setId: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: elementsKeys.list(setId),
    queryFn: ({ signal }) => listAllElements(setId, signal),
    enabled: session?.isAuthenticated === true,
  });
}
