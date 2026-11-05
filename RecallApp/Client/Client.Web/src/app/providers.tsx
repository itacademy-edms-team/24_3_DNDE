import { MantineProvider } from "@mantine/core";
import { MutationCache, QueryCache, QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";

import { handleGlobalError } from "./globalErrors";

/**
 * Корневые провайдеры приложения: тема/компоненты Mantine и кэш удалённых данных TanStack Query.
 * Глобальные обработчики ошибок дают единую реакцию на утрату сессии для запросов и мутаций.
 */
export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        queryCache: new QueryCache({ onError: handleGlobalError }),
        mutationCache: new MutationCache({ onError: handleGlobalError }),
        defaultOptions: {
          queries: {
            // Данные сессии/профиля не считаем устаревшими между переходами по страницам;
            // явная инвалидация по действиям (вход/выход).
            staleTime: 30_000,
            retry: false,
            refetchOnWindowFocus: false,
          },
        },
      }),
  );

  return (
    <MantineProvider defaultColorScheme="light">
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </MantineProvider>
  );
}
