import { useQuery } from "@tanstack/react-query";

import { useSession } from "../../session/api/useSession";
import { getMyAccount } from "./getMyAccount";
import { accountKeys } from "./accountKeys";

/** Профиль текущего пользователя из домена. Запрашивается только при активной сессии. */
export function useMyAccount() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: accountKeys.me,
    queryFn: ({ signal }) => getMyAccount(signal),
    enabled: session?.isAuthenticated === true,
    staleTime: 30_000,
  });
}
