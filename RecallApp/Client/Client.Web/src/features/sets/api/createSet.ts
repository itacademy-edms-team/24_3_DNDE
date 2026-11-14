import { apiClient } from "@/shared/api/clients/apiClient";
import type { CreateSetInput, SetSummary } from "@/features/sets/model/types";

/**
 * Создание набора (POST /api/sets). Заголовок Idempotency-Key обязателен для бэкенда:
 * один ключ - одна попытка вставки, повтор с тем же ключом не создаёт дубликат.
 */
export async function createSet(input: CreateSetInput): Promise<SetSummary> {
  const { data } = await apiClient.post<SetSummary>("/sets", input, {
    headers: { "Idempotency-Key": crypto.randomUUID() },
  });
  return data;
}
