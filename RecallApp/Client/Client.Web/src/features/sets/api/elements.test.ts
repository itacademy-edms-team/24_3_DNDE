import type { AxiosResponse } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/shared/api/clients/apiClient", () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));

import { apiClient } from "@/shared/api/clients/apiClient";
import { moveElement } from "./elements";

const post = vi.mocked(apiClient.post);

describe("elements api", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    post.mockResolvedValue({ data: {} } as AxiosResponse<unknown>);
  });

  it("moveElement posts the neighbours to /elements/{id}/move", async () => {
    await moveElement("s1", "e1", "e0", "e2");

    expect(post).toHaveBeenCalledWith("/sets/s1/elements/e1/move", {
      previousId: "e0",
      nextId: "e2",
    });
  });

  it("moveElement sends null for a list boundary", async () => {
    await moveElement("s1", "e1", null, "e2");

    expect(post).toHaveBeenCalledWith("/sets/s1/elements/e1/move", {
      previousId: null,
      nextId: "e2",
    });
  });
});
