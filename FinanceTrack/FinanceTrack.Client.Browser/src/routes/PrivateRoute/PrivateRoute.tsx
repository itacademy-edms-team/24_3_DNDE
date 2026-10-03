import { ReactNode } from 'react';
import { Box, CircularProgress } from '@mui/material';
import { useQuery } from '@tanstack/react-query';

import Unauthorized from '@/components/Unauthorized';

interface PrivateRouteProps {
  children: ReactNode;
}

type AuthStatus = 'authenticated' | 'unauthorized' | 'error';

const checkAuth = async (): Promise<AuthStatus> => {
  try {
    const response = await fetch('/bff/user', { method: 'GET', credentials: 'include' });
    if (response.status === 401) return 'unauthorized';
    if (response.ok) return 'authenticated';
    return 'error';
  } catch {
    return 'error';
  }
};

export function PrivateRoute({ children }: PrivateRouteProps) {
  const { data: status, isLoading } = useQuery({
    queryKey: ['auth-status'],
    queryFn: checkAuth,
    // Кэшируем в памяти результат на 5 минут. Даже когда компонент размонтируется, данные будут доступны 5 минут.
    gcTime: 5 * 60 * 1000,
    // Данные считаем устаревшими через 30 секунд после получения
    staleTime: 30 * 1000,
    // Отправляем запрос на проверку авторизации каждые 30 секунд, пока компонент смонтирован
    refetchInterval: 30 * 1000,
    // Убираем автоматические повторы при ошибках.
    retry: false,
  });

  if (isLoading) {
    return (
      <Box
        sx={{
          height: '100%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (status === 'authenticated') {
    return <>{children}</>;
  }

  if (status === 'unauthorized') {
    return <Unauthorized />;
  }

  // status === 'error' or undefined
  return (
    <Unauthorized message="Ошибка аутентификации. Войдите в аккаунт" />
  );
}
