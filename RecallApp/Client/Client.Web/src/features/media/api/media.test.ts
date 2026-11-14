import type { AxiosResponse } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/shared/api/clients/apiClient", () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), delete: vi.fn() },
}));

import { apiClient } from "@/shared/api/clients/apiClient";
import { confirmUpload, deleteMedia, getMedia, initUpload } from "./media";

const get = vi.mocked(apiClient.get);
const post = vi.mocked(apiClient.post);
const del = vi.mocked(apiClient.delete);

describe("media api", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    get.mockResolvedValue({ data: {} } as AxiosResponse<unknown>);
    post.mockResolvedValue({ data: {} } as AxiosResponse<unknown>);
    del.mockResolvedValue({} as AxiosResponse<unknown>);
  });

  it("initUpload posts to /media/uploads with an Idempotency-Key", async () => {
    await initUpload({ contentType: "image/png", sizeBytes: 100 });

    expect(post).toHaveBeenCalledWith(
      "/media/uploads",
      { contentType: "image/png", sizeBytes: 100 },
      expect.objectContaining({
        headers: expect.objectContaining({
          "Idempotency-Key": expect.stringMatching(/^[0-9a-f-]{36}$/i),
        }),
      }),
    );
  });

  it("confirmUpload posts to /media/uploads/{id}/confirm with an Idempotency-Key", async () => {
    await confirmUpload("m1");

    expect(post).toHaveBeenCalledWith(
      "/media/uploads/m1/confirm",
      undefined,
      expect.objectContaining({
        headers: expect.objectContaining({
          "Idempotency-Key": expect.stringMatching(/^[0-9a-f-]{36}$/i),
        }),
      }),
    );
  });

  it("getMedia gets /media/{id}", async () => {
    await getMedia("m1");
    expect(get).toHaveBeenCalledWith("/media/m1", { signal: undefined });
  });

  it("deleteMedia deletes /media/{id}", async () => {
    await deleteMedia("m1");
    expect(del).toHaveBeenCalledWith("/media/m1");
  });
});
