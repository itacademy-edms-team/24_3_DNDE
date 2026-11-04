import { NavLink, Stack } from "@mantine/core";
import { Link, useLocation } from "react-router";

/** Пункты основного меню приложения. */
const items = [
  { to: "/", label: "Главная", exact: true },
  { to: "/account", label: "Аккаунт", exact: false },
];

/** Левая навигационная панель приложения. Активный пункт определяется текущим маршрутом. */
export function MainNav({ onNavigate }: { onNavigate?: () => void }) {
  const { pathname } = useLocation();

  return (
    <Stack gap={4}>
      {items.map((item) => (
        <NavLink
          key={item.to}
          component={Link}
          to={item.to}
          label={item.label}
          active={
            item.exact ? pathname === item.to : pathname.startsWith(item.to)
          }
          onClick={onNavigate}
        />
      ))}
    </Stack>
  );
}
