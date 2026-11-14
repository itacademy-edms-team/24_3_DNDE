import type { AxiosResponse } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/shared/api/clients/apiClient", () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

import { apiClient } from "@/shared/api/clients/apiClient";
import type { SetSummary } from "@/features/sets/model/types";
import { createSet } from "./createSet";
import { deleteSet } from "./deleteSet";
import { listSets } from "./listSets";
import { updateSet } from "./updateSet";

const get = vi.mocked(apiClient.get);
const post = vi.mocked(apiClient.post);
const put = vi.mocked(apiClient.put);
const del = vi.mocked(apiClient.delete);

const sampleSet: SetSummary = {
  id: "s1",
  title: "T",
  description: null,
  createdAt: "2026-11-14T00:00:00Z",
  updatedAt: "2026-11-14T00:00:00Z",
};

describe("sets api", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    post.mockResolvedValue({ data: sampleSet } as AxiosResponse<SetSummary>);
    get.mockResolvedValue({ data: { items: [], nextCursor: null } } as AxiosResponse<unknown>);
    put.mockResolvedValue({ data: sampleSet } as AxiosResponse<SetSummary>);
    del.mockResolvedValue({} as AxiosResponse<unknown>);
  });

  it("createSet posts to /sets and sends an Idempotency-Key", async () => {
    await createSet({ title: "T", description: null });

    expect(post).toHaveBeenCalledWith(
      "/sets",
      { title: "T", description: null },
      expect.objectContaining({
        headers: expect.objectContaining({
          "Idempotency-Key": expect.stringMatching(/^[0-9a-f-]{36}$/i),
        }),
      }),
    );
  });

  it("listSets omits cursor for the first page and passes it afterwards", async () => {
    await listSets(null);
    expect(get).toHaveBeenLastCalledWith(
      "/sets",
      expect.objectContaining({ params: undefined }),
    );

    await listSets("cursor-1");
    expect(get).toHaveBeenLastCalledWith(
      "/sets",
      expect.objectContaining({ params: { cursor: "cursor-1" } }),
    );
  });

  it("updateSet puts the full body to /sets/{id}", async () => {
    await updateSet("s1", { title: "New", description: "D" });
    expect(put).toHaveBeenCalledWith("/sets/s1", { title: "New", description: "D" });
  });

  it("deleteSet deletes /sets/{id}", async () => {
    await deleteSet("s1");
    expect(del).toHaveBeenCalledWith("/sets/s1");
  });
});
