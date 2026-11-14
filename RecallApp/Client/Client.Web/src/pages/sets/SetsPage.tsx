import { useEffect, useState } from "react";
import {
  Button,
  Center,
  Group,
  Loader,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { useIntersection } from "@mantine/hooks";
import { Plus } from "@phosphor-icons/react";

import { toErrorView } from "@/shared/api/errors/errorView";
import { RequireSession } from "@/features/session/ui/RequireSession";
import { useSets } from "@/features/sets/api/useSets";
import { DeleteSetModal } from "@/features/sets/ui/DeleteSetModal";
import { SetCard } from "@/features/sets/ui/SetCard";
import { SetFormModal } from "@/features/sets/ui/SetFormModal";
import type { SetSummary } from "@/features/sets/model/types";

/** Модалка набора: закрыта, создание или редактирование конкретного набора. */
type SetModalState =
  | { kind: "closed" }
  | { kind: "create" }
  | { kind: "edit"; set: SetSummary };

/** Страница «Мои наборы»: бесконечный список + создание/редактирование/удаление. */
export function SetsPage() {
  return (
    <RequireSession>
      <SetsContent />
    </RequireSession>
  );
}

function SetsContent() {
  const {
    data,
    isPending,
    isError,
    error,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  } = useSets();

  const [modal, setModal] = useState<SetModalState>({ kind: "closed" });
  const [deleting, setDeleting] = useState<SetSummary | null>(null);

  const { ref: sentinelRef, entry } = useIntersection({ threshold: 0.1 });

  useEffect(() => {
    if (entry?.isIntersecting && hasNextPage && !isFetchingNextPage) {
      void fetchNextPage();
    }
  }, [entry?.isIntersecting, hasNextPage, isFetchingNextPage, fetchNextPage]);

  const items = data?.pages.flatMap((page) => page.items) ?? [];

  if (isPending) {
    return (
      <Center h="50vh">
        <Loader />
      </Center>
    );
  }

  if (isError) {
    const view = toErrorView(error);
    return (
      <Center h="50vh">
        <Stack gap="xs" align="center">
          <Text fw={600}>Не удалось загрузить наборы</Text>
          <Text c="dimmed" size="sm">
            {view.message}
          </Text>
        </Stack>
      </Center>
    );
  }

  return (
    <Stack gap="md">
      <Group justify="space-between" align="center">
        <Title order={2}>Мои наборы</Title>
        <Button
          leftSection={<Plus size={16} />}
          onClick={() => setModal({ kind: "create" })}
        >
          Создать набор
        </Button>
      </Group>

      {items.length === 0 ? (
        <Center py="xl">
          <Text c="dimmed">Наборов пока нет создайте первый.</Text>
        </Center>
      ) : (
        <Stack gap="sm">
          {items.map((set) => (
            <SetCard
              key={set.id}
              set={set}
              onRename={(s) => setModal({ kind: "edit", set: s })}
              onDelete={setDeleting}
            />
          ))}
        </Stack>
      )}

      {/* Сентинел догрузки: когда виден и есть следующая страница  fetchNextPage. */}
      <div ref={sentinelRef} style={{ height: 1 }} />

      {isFetchingNextPage && (
        <Center py="sm">
          <Loader size="sm" />
        </Center>
      )}

      {modal.kind !== "closed" && (
        <SetFormModal
          set={modal.kind === "edit" ? modal.set : null}
          onClose={() => setModal({ kind: "closed" })}
        />
      )}
      {deleting && (
        <DeleteSetModal set={deleting} onClose={() => setDeleting(null)} />
      )}
    </Stack>
  );
}
