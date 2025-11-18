import { Button, Typography } from '@mui/material';
import { useEffect, useState } from 'react';

type UserInfo = {
  name: string;
  claims: { type: string; value: string }[];
};

function LoginPage() {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);

  const loadUser = async () => {
    setLoading(true);
    try {
      const res = await fetch('/bff/user', {
        credentials: 'include', // Include cookies
      });
      if (res.status === 401) {
        setUser(null);
      } else {
        const data = await res.json();
        setUser(data);
      }
    } catch (e) {
      console.error(e);
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUser();
  }, []);

  const handleLogin = () => {
    window.location.href = '/bff/login';
  };

  const handleLogout = () => {
    window.location.href = '/bff/logout';
  };

  if (loading) {
    return <Typography>Loading...</Typography>;
  }

  const isAuth = !!user;

  return (
    <>
      <Typography variant="h3">Login</Typography>
      {!isAuth && (
        <Button onClick={handleLogin}>Login</Button>
      )}
      {isAuth && (
        <>
          <Typography variant="body1">
            Hello, {user.name}
          </Typography>
          <Button onClick={handleLogout}>
            Logout
          </Button>
        </>
      )}
    </>
  );
}

export default LoginPage;
