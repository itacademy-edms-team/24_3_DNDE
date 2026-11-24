import { Button, Group, Modal, Stack, Text } from "@mantine/core";

import { toErrorView } from "@/shared/api/errors/errorView";
import { useDeleteSet } from "@/features/sets/api/useSetMutations";
import type { SetSummary } from "@/features/sets/model/types";

interface DeleteSetModalProps {
  set: SetSummary;
  onClose: () => void;
}

/** Подтверждение удаления набора. Монтируется только когда открыта. */
export function DeleteSetModal({ set, onClose }: DeleteSetModalProps) {
  const del = useDeleteSet();
  const serverView = del.error ? toErrorView(del.error) : null;

  return (
    <Modal opened onClose={onClose} title="Удалить набор?" centered>
      <Stack gap="md">
        <Text size="sm">
          Набор «{set.title}» и все его карточки будут удалены. Это действие нельзя отменить.
        </Text>
        {serverView && (
          <Text c="red" size="sm">
            {serverView.message}
          </Text>
        )}
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>
            Отмена
          </Button>
          <Button
            color="red"
            loading={del.isPending}
            onClick={() => del.mutate(set.id, { onSuccess: onClose })}
          >
            Удалить
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}
