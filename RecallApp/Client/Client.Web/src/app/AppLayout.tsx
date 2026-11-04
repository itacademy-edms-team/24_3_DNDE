import { AppShell, Burger, Container } from "@mantine/core";
import { useDisclosure, useMediaQuery } from "@mantine/hooks";
import { Outlet } from "react-router";

import { SessionMenu } from "@/features/session/ui/SessionMenu";
import { AppHeader } from "@/shared/ui/AppHeader";
import { MainNav } from "./MainNav";

/**
 * App layout.
 */
export function AppLayout() {
  // getInitialValueInEffect: false - значение медиа-запроса читается сразу из matchMedia.
  // SPA не рендерится на сервере, поэтому рассинхрона при гидратации нет,
  // зато на мобильном панель сразу стартует скрытой, без вспышки открытой панели.
  const isMobile = useMediaQuery("(max-width: 48em)", false, {
    getInitialValueInEffect: false,
  });
  const [navOpened, nav] = useDisclosure(!isMobile);

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{
        width: 240,
        breakpoint: "sm",
        collapsed: { desktop: !navOpened, mobile: !navOpened },
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
          right={<SessionMenu />}
        />
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <MainNav onNavigate={nav.close} />
      </AppShell.Navbar>

      <AppShell.Main>
        <Container size="lg" py="md">
          <Outlet />
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}
