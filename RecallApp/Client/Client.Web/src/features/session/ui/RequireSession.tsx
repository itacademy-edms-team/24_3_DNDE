import { Center, Loader, Stack, Text } from "@mantine/core";
import { useEffect } from "react";
import { useLocation } from "react-router";

import { toErrorView } from "../../../shared/api/errors/errorView";
import { useSession } from "../api/useSession";
import { loginUrl } from "../model/sessionUrls";

/**
 * Пропускает дальше только пользователя с активной сессией.
 * Без сессии уводит на вход (полная навигация на BFF) с возвратом на текущий путь.
 */
export function RequireSession({ children }: { children: React.ReactNode }) {
  const location = useLocation();
  const { data, isPending, isError, error } = useSession();

  const isAuthenticated = data?.isAuthenticated === true;
  const returnTo = `${location.pathname}${location.search}`;
  const mustLogin = !isPending && !isAuthenticated;

  useEffect(() => {
    if (mustLogin) {
      window.location.assign(loginUrl(returnTo));
    }
  }, [mustLogin, returnTo]);

  if (isPending) {
    return (
      <Center h="50vh">
        <Loader />
      </Center>
    );
  }

  if (isError) {
    const view = toErrorView(error);
    return (
      <Center h="50vh">
        <Stack gap="xs" align="center">
          <Text fw={600}>Не удалось проверить сессию</Text>
          <Text c="dimmed" size="sm">
            {view.message}
          </Text>
        </Stack>
      </Center>
    );
  }

  if (!isAuthenticated) {
    return (
      <Center h="50vh">
        <Text c="dimmed">Перенаправляем на вход…</Text>
      </Center>
    );
  }

  return <>{children}</>;
}
