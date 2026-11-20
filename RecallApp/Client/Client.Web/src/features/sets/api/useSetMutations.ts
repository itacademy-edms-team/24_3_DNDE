import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { UpdateSetInput } from "@/features/sets/model/types";
import { createSet } from "./createSet";
import { deleteSet } from "./deleteSet";
import { setsKeys } from "./setsKeys";
import { updateSet } from "./updateSet";

/** Создание набора с инвалидацией списка: новый набор появится в начале (createdAt DESC). */
export function useCreateSet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createSet,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: setsKeys.list() }),
  });
}

/** Обновление набора (название/описание) с инвалидацией списка. */
export function useUpdateSet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSetInput }) => updateSet(id, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: setsKeys.list() }),
  });
}

/** Удаление набора с инвалидацией списка. */
export function useDeleteSet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteSet,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: setsKeys.list() }),
  });
}
