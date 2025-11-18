import { Button, Typography } from '@mui/material';
import { useKeycloak } from '@react-keycloak/web';

function LoginPage() {
  const { keycloak } = useKeycloak();

  return (
    <>
      <meta name="title" content="Page 1" />
      <Typography variant="h3">Login</Typography>
      {!keycloak.authenticated && (
        <Button onClick={() => keycloak.login()}>Login</Button>
      )}
      {!!keycloak.authenticated && (
        <Button onClick={() => keycloak.logout()}>Logout ({keycloak!.tokenParsed!.preferred_username})</Button>
      )}
    </>
  );
}

export default LoginPage;
