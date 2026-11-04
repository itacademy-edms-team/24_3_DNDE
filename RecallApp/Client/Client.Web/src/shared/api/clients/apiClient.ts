import { apiBaseUrl } from "@/shared/api/apiConfig";
import { createRestClient } from "./createRestClient";

/** Клиент API домена: запросы идут через BFF (YARP -> монолит). */
export const apiClient = createRestClient(apiBaseUrl);
