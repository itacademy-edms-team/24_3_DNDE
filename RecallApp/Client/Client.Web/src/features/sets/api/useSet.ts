import { useQuery } from "@tanstack/react-query";

import { useSession } from "@/features/session/api/useSession";
import { getSet } from "./getSet";
import { setsKeys } from "./setsKeys";

/** Набор по id (заголовок редактора). */
export function useSet(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: setsKeys.detail(id),
    queryFn: ({ signal }) => getSet(id, signal),
    enabled: session?.isAuthenticated === true,
  });
}
