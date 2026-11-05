import { Center, Loader, Stack, Text } from "@mantine/core";
import { useEffect } from "react";
import { useLocation } from "react-router";

import { toErrorView } from "@/shared/api/errors/errorView";
import { useSession } from "@/features/session/api/useSession";
import {
  canRedirectToLogin,
  markLoginRedirect,
  resetAuthRedirects,
} from "@/features/session/model/authRedirectGuard";
import { loginUrl } from "@/features/session/model/sessionUrls";

/**
 * Пропускает дальше только пользователя с активной сессией.
 * Без сессии уводит на вход (полная навигация на BFF) с возвратом на текущий путь.
 * Повторный возврат без сессии не создаёт цикл из редиректов: показываем ошибку (authRedirectGuard).
 */
export function RequireSession({ children }: { children: React.ReactNode }) {
  const location = useLocation();
  const { data, isPending, isError, error } = useSession();

  const isAuthenticated = data?.isAuthenticated === true;
  const returnTo = `${location.pathname}${location.search}`;
  const mustLogin = !isPending && !isAuthenticated;
  // Решается в рендере: бюджет попыток держим в sessionStorage, без побочных эффектов.
  const redirectBlocked = mustLogin && !canRedirectToLogin();

  useEffect(() => {
    if (isAuthenticated) resetAuthRedirects();
  }, [isAuthenticated]);

  useEffect(() => {
    if (!mustLogin || redirectBlocked) return;

    markLoginRedirect();
    window.location.assign(loginUrl(returnTo));
  }, [mustLogin, redirectBlocked, returnTo]);

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
        <Stack gap="xs" align="center" px="md">
          <Text fw={600}>
            {redirectBlocked ? "Не удалось установить сессию" : "Перенаправляем на вход..."}
          </Text>
          {redirectBlocked && (
            <Text c="dimmed" size="sm" ta="center">
              Вход был выполнен успешно, но не можем получить статус по сессии с BFF-адреса. 
              Выполните перезагрузку с очисткой куки и попробуйте войти ещё раз.
              Иначе обратитесь в нашу техническую поддержку.
            </Text>
          )}
        </Stack>
      </Center>
    );
  }

  return <>{children}</>;
}
