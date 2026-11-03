import { MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";

/**
 * Корневые провайдеры приложения: тема/компоненты Mantine и серверное состояние TanStack Query.
 */
export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
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
    <MantineProvider defaultColorScheme="auto">
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </MantineProvider>
  );
}
