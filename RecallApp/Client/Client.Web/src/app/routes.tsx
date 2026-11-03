import type { RouteObject } from "react-router";

import { AccountPage } from "../pages/account/AccountPage";
import { HomePage } from "../pages/home/HomePage";
import { AppLayout } from "./AppLayout";
import { NotFoundPage } from "./NotFoundPage";

/** Дерево маршрутов без привязки к истории: используется браузерным роутером и тестами. */
export const routes: RouteObject[] = [
  {
    path: "/",
    element: <AppLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "account", element: <AccountPage /> },
      { path: "*", element: <NotFoundPage /> },
    ],
  },
];
