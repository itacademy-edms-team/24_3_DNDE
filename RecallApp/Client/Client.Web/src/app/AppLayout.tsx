import { AppShell, Container } from "@mantine/core";
import { Outlet } from "react-router";

import { useSession } from "../features/session/api/useSession";
import { LoginButton } from "../features/session/ui/LoginButton";
import { LogoutButton } from "../features/session/ui/LogoutButton";
import { AppHeader } from "../shared/ui/AppHeader";

/** Каркас приложения: шапка с действиями по состоянию сессии и область страницы. */
export function AppLayout() {
  const { data: session } = useSession();
  const isAuthenticated = session?.isAuthenticated === true;

  return (
    <AppShell header={{ height: 60 }} padding={0}>
      <AppShell.Header>
        <AppHeader actions={isAuthenticated ? <LogoutButton /> : <LoginButton />} />
      </AppShell.Header>
      <AppShell.Main>
        <Container size="lg" py="lg">
          <Outlet />
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}
