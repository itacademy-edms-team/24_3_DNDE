import { createBrowserRouter } from "react-router";

import { routes } from "./routes";

/**
 * Браузерный роутер: синхронизирован с History API браузера.
 * Компоненты читают текущий маршрут через useLocation, код вне React - через router.state.location.
 */
export const router = createBrowserRouter(routes);
