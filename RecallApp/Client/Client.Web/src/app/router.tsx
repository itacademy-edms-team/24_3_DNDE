import { createBrowserRouter } from "react-router";

import { routes } from "./routes";

/** Браузерный роутер приложения: история и URL берутся из адресной строки. */
export const router = createBrowserRouter(routes);
