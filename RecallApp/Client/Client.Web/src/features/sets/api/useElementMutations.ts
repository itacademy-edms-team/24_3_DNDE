import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { CreateElementInput, UpdateElementInput } from "@/features/sets/model/types";
import { createElement, deleteElement, reorderElements, updateElement } from "./elements";
import { elementsKeys } from "./elementsKeys";

/**
 * Мутации элементов. onSuccess инвалидирует кэш списка, чтобы повторное открытие редактора
 * видело свежий список. Локальное состояние EditorBoard это не трогает: он инициализируется
 * один раз (useState-инициализатор) и не перечитывает кэш после маунта.
 */
export function useCreateElement(setId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateElementInput) => createElement(setId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: elementsKeys.list(setId) }),
  });
}

export function useUpdateElement(setId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ elementId, input }: { elementId: string; input: UpdateElementInput }) =>
      updateElement(setId, elementId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: elementsKeys.list(setId) }),
  });
}

export function useDeleteElement(setId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (elementId: string) => deleteElement(setId, elementId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: elementsKeys.list(setId) }),
  });
}

export function useReorderElements(setId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (elementIds: string[]) => reorderElements(setId, elementIds),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: elementsKeys.list(setId) }),
  });
}
