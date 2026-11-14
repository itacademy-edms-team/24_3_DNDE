import type { RouteObject } from "react-router";

import { AccountPage } from "@/pages/account/AccountPage";
import { HomePage } from "@/pages/home/HomePage";
import { EditSetPage } from "@/pages/sets/EditSetPage";
import { SetsPage } from "@/pages/sets/SetsPage";
import { AppLayout } from "./AppLayout";
import { NotFoundPage } from "./NotFoundPage";

/**
 * Дерево маршрутов без привязки к истории.
 * Отделено от экземпляра роутера, чтобы тесты строили createMemoryRouter поверх того же дерева,
 * а компоненты получали адрес и навигацию только через хуки react-router.
 */
export const routes: RouteObject[] = [
  {
    path: "/",
    element: <AppLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "account", element: <AccountPage /> },
      { path: "sets", element: <SetsPage /> },
      { path: "sets/:id/edit", element: <EditSetPage /> },
      { path: "*", element: <NotFoundPage /> },
    ],
  },
];
