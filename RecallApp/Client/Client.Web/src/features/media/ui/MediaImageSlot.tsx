import { ActionIcon, Box, Group, Loader, Text } from "@mantine/core";
import { Plus, Trash } from "@phosphor-icons/react";
import { useRef, useState } from "react";

import {
  confirmUpload,
  initUpload,
  uploadToS3,
} from "@/features/media/api/media";
import {
  MEDIA_CONTENT_TYPES,
  MEDIA_MAX_BYTES,
} from "@/features/media/model/types";
import { toErrorView } from "@/shared/api/errors/errorView";

/** Готовое изображение слота: id файла + превью (thumb) для отображения. */
export interface MediaSlotImage {
  mediaId: string;
  thumbUrl: string;
}

interface MediaImageSlotProps {
  image: MediaSlotImage | null;
  /** null — изображение удалено. */
  onChange: (image: MediaSlotImage | null) => void;
  label: string;
}

/** Слот изображения: загрузка в две стадии (init → S3 → confirm) с прогрессом + превью. */
export function MediaImageSlot({
  image,
  onChange,
  label,
}: MediaImageSlotProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const pick = () => {
    if (!uploading) inputRef.current?.click();
  };

  const handleFile = async (file: File) => {
    if (!(MEDIA_CONTENT_TYPES as readonly string[]).includes(file.type)) {
      setError("Поддерживаются только PNG, JPEG или WebP.");
      return;
    }
    if (file.size < 1 || file.size > MEDIA_MAX_BYTES) {
      setError("Файл должен быть от 1 байта до 10 МБ.");
      return;
    }

    setError(null);
    setUploading(true);
    setProgress(0);
    try {
      const init = await initUpload({
        contentType: file.type,
        sizeBytes: file.size,
      });
      await uploadToS3(init.url, init.fields, file, (value) =>
        setProgress(value),
      );
      const confirmed = await confirmUpload(init.mediaId);
      if (confirmed.status !== "ready" || !confirmed.variants.thumb) {
        setError("Не удалось обработать изображение.");
        return;
      }
      onChange({
        mediaId: confirmed.mediaId,
        thumbUrl: confirmed.variants.thumb.url,
      });
    } catch (error) {
      setError(toErrorView(error).message);
    } finally {
      setUploading(false);
      setProgress(0);
    }
  };

  return (
    <Box>
      {image ? (
        <Group align="flex-start" gap="xs">
          <img
            src={image.thumbUrl}
            alt={label}
            onClick={pick}
            style={{
              maxWidth: "100%",
              maxHeight: 200,
              borderRadius: 8,
              objectFit: "contain",
              cursor: "pointer",
            }}
            title="Нажмите, чтобы заменить"
          />
          <ActionIcon
            variant="subtle"
            color="red"
            onClick={() => onChange(null)}
            aria-label={`Удалить изображение: ${label}`}
          >
            <Trash size={16} />
          </ActionIcon>
        </Group>
      ) : (
        <Box
          onClick={pick}
          style={{
            border: "1px dashed var(--mantine-color-gray-4)",
            borderRadius: 8,
            padding: "var(--mantine-spacing-md)",
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8,
            color: "var(--mantine-color-dimmed)",
          }}
        >
          <Plus size={16} />
          <Text size="sm">Добавить изображение</Text>
        </Box>
      )}

      {uploading && (
        <Group gap="xs" align="center" mt="xs">
          <Loader size="xs" />
          <Text size="xs" c="dimmed">
            {progress > 0
              ? `Загрузка ${Math.round(progress * 100)}%`
              : "Обработка…"}
          </Text>
        </Group>
      )}

      {error && (
        <Text c="red" size="xs" mt="xs">
          {error}
        </Text>
      )}

      <input
        ref={inputRef}
        type="file"
        accept="image/png,image/jpeg,image/webp"
        hidden
        onChange={(event) => {
          const file = event.target.files?.[0];
          if (file) void handleFile(file);
          event.target.value = "";
        }}
      />
    </Box>
  );
}
