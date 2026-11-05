import { useQuery } from "@tanstack/react-query";

import { getSession } from "./getSession";
import { sessionKeys } from "./sessionKeys";

/** Текущее состояние сессии пользователя. */
export function useSession() {
  return useQuery({
    queryKey: sessionKeys.current,
    queryFn: ({ signal }) => getSession(signal),
    staleTime: 30_000,
  });
}
