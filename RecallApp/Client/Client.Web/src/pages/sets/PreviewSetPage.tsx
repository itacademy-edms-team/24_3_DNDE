import { useEffect, useRef, useState } from "react";
import type { KeyboardEvent, PointerEvent } from "react";
import {
  ActionIcon,
  Button,
  Center,
  Group,
  Loader,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { ArrowLeftIcon, ArrowRightIcon } from "@phosphor-icons/react";
import { Link, useParams } from "react-router";

import { toErrorView } from "@/shared/api/errors/errorView";
import { RequireSession } from "@/features/session/ui/RequireSession";
import { useSet } from "@/features/sets/api/useSet";
import { useElementsPaged } from "@/features/sets/api/useElementsPaged";
import {
  useMediaLink,
  useMediaLinks,
} from "@/features/media/api/useMediaLinks";
import type { Element } from "@/features/sets/model/types";

const SWIPE_THRESHOLD = 50;
const TAP_THRESHOLD = 10;

/** Файлы карточки: по ним переподписываются ссылки превью. */
function elementMediaIds(element: Element): string[] {
  switch (element.type) {
    case "flashcard": {
      const { frontImageId, backImageId } = element.payload;
      return [frontImageId, backImageId].filter((mediaId): mediaId is string =>
        Boolean(mediaId),
      );
    }
  }
}

/** Страница предпросмотра набора: read-only, флип-карточки по одной. */
export function PreviewSetPage() {
  const { id } = useParams();
  if (!id) return null;
  return (
    <RequireSession>
      <PreviewContent setId={id} />
    </RequireSession>
  );
}

function PreviewContent({ setId }: { setId: string }) {
  const { data: set } = useSet(setId);
  const {
    data,
    isPending,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useElementsPaged(setId);

  const [index, setIndex] = useState(0);

  const elements = data?.pages.flatMap((page) => page.items) ?? [];
  const total = set?.elementCount ?? elements.length;

  // Ссылки display-вариантов живут 15 минут: продлеваем батчем по всем загруженным страницам.
  useMediaLinks(elements.flatMap(elementMediaIds));

  // Догружаем следующую страницу заранее, когда подходим к концу загруженных карточек.
  useEffect(() => {
    if (
      elements.length > 0 &&
      index >= elements.length - 2 &&
      hasNextPage &&
      !isFetchingNextPage
    ) {
      void fetchNextPage();
    }
  }, [index, elements.length, hasNextPage, isFetchingNextPage, fetchNextPage]);

  if (isPending) {
    return (
      <Center h="50vh">
        <Loader />
      </Center>
    );
  }

  if (!data) {
    const view = toErrorView(error);
    return (
      <Center h="50vh">
        <Stack gap="xs" align="center">
          <Text fw={600}>Не удалось загрузить набор</Text>
          <Text c="dimmed" size="sm">
            {view.message}
          </Text>
        </Stack>
      </Center>
    );
  }

  const goTo = (i: number) => setIndex(Math.max(0, Math.min(total - 1, i)));
  const current = elements[index];

  return (
    <Stack gap="md">
      <Group justify="space-between" align="center">
        <Title order={2}>{set?.title ?? "Набор"}</Title>
        <Button
          variant="default"
          component={Link}
          to="/sets"
          leftSection={<ArrowLeftIcon size={16} />}
        >
          Назад
        </Button>
      </Group>

      {total === 0 ? (
        <Center py="xl">
          <Text c="dimmed">Набор пуст.</Text>
        </Center>
      ) : current ? (
        <Stack gap="md" align="center">
          <FlipCard
            key={current.id}
            element={current}
            onPrev={() => goTo(index - 1)}
            onNext={() => goTo(index + 1)}
          />
          <Group gap="md" align="center">
            <ActionIcon
              variant="default"
              size="lg"
              onClick={() => goTo(index - 1)}
              disabled={index === 0}
              aria-label="Предыдущая карточка"
            >
              <ArrowLeftIcon size={20} />
            </ActionIcon>
            <Text c="dimmed">
              {index + 1} / {total}
            </Text>
            <ActionIcon
              variant="default"
              size="lg"
              onClick={() => goTo(index + 1)}
              disabled={index >= total - 1}
              aria-label="Следующая карточка"
            >
              <ArrowRightIcon size={20} />
            </ActionIcon>
          </Group>
        </Stack>
      ) : (
        <Center py="xl">
          <Loader />
        </Center>
      )}
    </Stack>
  );
}

interface FlipCardProps {
  element: Element;
  onPrev: () => void;
  onNext: () => void;
}

/**
 * Флип-карточка: тап переворачивает, свайп влево/вправо листает.
 * `flipped` локальный: при смене карточки (key по id) компонент перемонтируется лицевой стороной
 * без проигрывания обратного флипа. Без оценок — это CORE-01 позже.
 */
function FlipCard({ element, onPrev, onNext }: FlipCardProps) {
  const [flipped, setFlipped] = useState(false);
  const start = useRef<{ x: number; y: number } | null>(null);

  const handlePointerDown = (event: PointerEvent<HTMLDivElement>) => {
    start.current = { x: event.clientX, y: event.clientY };
  };

  const handlePointerUp = (event: PointerEvent<HTMLDivElement>) => {
    if (!start.current) return;
    const dx = event.clientX - start.current.x;
    const dy = event.clientY - start.current.y;
    start.current = null;

    if (Math.abs(dx) > SWIPE_THRESHOLD && Math.abs(dx) > Math.abs(dy)) {
      if (dx > 0) onPrev();
      else onNext();
    } else if (Math.abs(dx) < TAP_THRESHOLD && Math.abs(dy) < TAP_THRESHOLD) {
      setFlipped((f) => !f);
    }
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === "ArrowLeft") onPrev();
    else if (event.key === "ArrowRight") onNext();
    else if (event.key === " " || event.key === "Enter") {
      event.preventDefault();
      setFlipped((f) => !f);
    }
  };

  const frontImage = element.images?.front?.display?.url;
  const backImage = element.images?.back?.display?.url;

  return (
    <div
      onPointerDown={handlePointerDown}
      onPointerUp={handlePointerUp}
      onKeyDown={handleKeyDown}
      role="button"
      tabIndex={0}
      aria-label="Карточка: нажмите, чтобы перевернуть"
      style={{
        perspective: "1200px",
        width: "100%",
        maxWidth: 420,
        cursor: "pointer",
        outline: "none",
      }}
    >
      <div
        style={{
          position: "relative",
          width: "100%",
          aspectRatio: "3 / 4",
          transformStyle: "preserve-3d",
          transition: "transform 0.6s",
          transform: flipped ? "rotateY(180deg)" : "rotateY(0deg)",
        }}
      >
        <FlipFace
          mediaId={element.payload.frontImageId ?? undefined}
          image={frontImage}
          text={element.payload.term}
        />
        <FlipFace
          mediaId={element.payload.backImageId ?? undefined}
          image={backImage}
          text={element.payload.definition}
          back
        />
      </div>
    </div>
  );
}

function FlipFace({
  mediaId,
  image,
  text,
  back,
}: {
  mediaId?: string;
  image?: string;
  text: string;
  back?: boolean;
}) {
  const { variants, refresh } = useMediaLink(mediaId ?? "");
  // Свежая ссылка из батча перекрывает ту, что пришла с элементом.
  const src = variants?.display?.url ?? image;
  // Одна фолбэк-попытка на файл: иначе битая ссылка зациклит перезагрузку.
  const attempted = useRef<string | null>(null);

  const handleLoadError = () => {
    if (!mediaId || attempted.current === mediaId) return;
    attempted.current = mediaId;
    void refresh();
  };

  return (
    <div
      style={{
        position: "absolute",
        inset: 0,
        backfaceVisibility: "hidden",
        WebkitBackfaceVisibility: "hidden",
        transform: back ? "rotateY(180deg)" : undefined,
        borderRadius: 12,
        border: "1px solid var(--mantine-color-gray-3)",
        background: "var(--mantine-color-body)",
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      {src && (
        <img
          src={src}
          alt=""
          onError={handleLoadError}
          style={{ width: "100%", height: "55%", objectFit: "cover" }}
        />
      )}
      <div
        style={{
          flex: 1,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          padding: 16,
          overflowY: "auto",
        }}
      >
        <Text
          fw={600}
          size="lg"
          ta="center"
          style={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}
        >
          {text}
        </Text>
      </div>
    </div>
  );
}
