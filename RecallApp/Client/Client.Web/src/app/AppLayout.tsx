import { AppShell, Burger, Button, Container } from "@mantine/core";
import { useDisclosure, useMediaQuery } from "@mantine/hooks";
import { Outlet } from "react-router";

import { useSession } from "../features/session/api/useSession";
import { SessionPanel } from "../features/session/ui/SessionPanel";
import { AppHeader } from "../shared/ui/AppHeader";
import { MainNav } from "./MainNav";

/**
 * Каркас приложения: шапка с двумя переключателями, скрывающаяся навигация слева,
 * скрывающаяся панель пользователя справа и область страницы.
 */
export function AppLayout() {
  // getInitialValueInEffect: false - значение медиа-запроса читается сразу из matchMedia.
  // SPA не рендерится на сервере, поэтому рассинхрона при гидратации нет,
  // зато на мобильном панель сразу стартует скрытой, без вспышки открытой панели.
  const isMobile = useMediaQuery("(max-width: 48em)", false, {
    getInitialValueInEffect: false,
  });
  const [navOpened, nav] = useDisclosure(!isMobile);
  const [panelOpened, panel] = useDisclosure(false);
  const { data: session } = useSession();

  const isAuthenticated = session?.isAuthenticated === true;
  const panelLabel = isAuthenticated
    ? (session.name ?? session.email ?? "Аккаунт")
    : "Войти";

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{
        width: 240,
        breakpoint: "sm",
        collapsed: { desktop: !navOpened, mobile: !navOpened },
      }}
      aside={{
        width: 280,
        breakpoint: "sm",
        collapsed: { desktop: !panelOpened, mobile: !panelOpened },
      }}
      padding="md"
    >
      <AppShell.Header>
        <AppHeader
          left={
            <Burger
              opened={navOpened}
              onClick={nav.toggle}
              size="sm"
              aria-label={navOpened ? "Скрыть меню" : "Показать меню"}
            />
          }
          right={
            <Button variant="subtle" size="compact-md" onClick={panel.toggle}>
              {panelLabel}
            </Button>
          }
        />
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <MainNav onNavigate={nav.close} />
      </AppShell.Navbar>

      <AppShell.Aside p="md">
        <SessionPanel onNavigate={panel.close} />
      </AppShell.Aside>

      <AppShell.Main>
        <Container size="lg" py="md">
          <Outlet />
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}
