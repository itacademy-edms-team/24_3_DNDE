import { NavLink, Stack } from "@mantine/core";
import { Link, useMatch } from "react-router";

/** Пункт основного меню. end - точное совпадение пути (нужно для корневого маршрута). */
interface NavItem {
  to: string;
  label: string;
  end: boolean;
}

const items: NavItem[] = [
  { to: "/", label: "Главная", end: true },
  { to: "/account", label: "Аккаунт", end: false },
];

/** Левая навигация приложения: активный пункт определяет роутер, а не сравнение строк. */
export function MainNav({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <Stack gap={4}>
      {items.map((item) => (
        <MainNavItem key={item.to} item={item} onNavigate={onNavigate} />
      ))}
    </Stack>
  );
}

/** Пункт меню: активность из useMatch (учитывает вложенность и параметры маршрута). */
function MainNavItem({ item, onNavigate }: { item: NavItem; onNavigate?: () => void }) {
  const match = useMatch({ path: item.to, end: item.end });
  const active = match !== null;

  return (
    <NavLink
      component={Link}
      to={item.to}
      label={item.label}
      active={active}
      aria-current={active ? "page" : undefined}
      onClick={onNavigate}
    />
  );
}
