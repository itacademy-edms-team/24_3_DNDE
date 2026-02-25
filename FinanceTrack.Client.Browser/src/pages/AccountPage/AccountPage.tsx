import { FullSizeCentered } from '@/components/styled';
import { Button, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';

import Loading from '@/components/Loading';

type UserInfo = {
  name: string;
  claims: { type: string; value: string }[];
};

const fetchUser = async (): Promise<UserInfo | null> => {
  try {
    const res = await fetch('/bff/user', {
      credentials: 'include', // Include cookies
    });
    if (res.status === 401) {
      return null;
    }
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`);
    }
    return await res.json();
  } catch (e) {
    console.error('Failed to fetch user:', e);
    return null;
  }
};

function AccountPage() {
  const { data: user, isLoading, isPending, error } = useQuery({
    queryKey: ['user'],
    queryFn: fetchUser,
    retry: false,
  });

  if (error) {
    console.error('Query error:', error);
  }

  const handleLogin = () => {
    window.location.href = '/bff/login';
  };

  const handleLogout = () => {
    window.location.href = '/bff/logout';
  };

  if (isLoading || isPending) {
    return <Loading />;
  }

  const isAuth = !!user;

  return (
    <>
      <meta name="title" content="Account" />
      {!isAuth && (
        <Button onClick={handleLogin}>Войти</Button>
      )}
      {isAuth && (
        <FullSizeCentered>
          <Typography variant="h3">Аккаунт</Typography>
          <Typography variant="body1">
            Здравствуйте, {user.name}
          </Typography>
          <Button onClick={handleLogout}>
            Выйти
          </Button>
        </FullSizeCentered>
      )}
    </>
  );
}

export default AccountPage;
