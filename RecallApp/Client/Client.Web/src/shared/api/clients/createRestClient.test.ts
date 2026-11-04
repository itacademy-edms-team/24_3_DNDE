import { AxiosError, AxiosHeaders, type InternalAxiosRequestConfig } from "axios";
import { describe, expect, it } from "vitest";

import { createRestClient } from "./createRestClient";
import { RestApiError } from "@/shared/api/errors/restApiError";

/** Adapter, который отвечает заданной ошибкой, не выходя в сеть. */
function failingAdapter(status: number, data: unknown) {
  return async (config: InternalAxiosRequestConfig) => {
    const headers = new AxiosHeaders({ "Content-Type": "application/problem+json" });
    throw new AxiosError(
      "Request failed with status code " + status,
      String(status),
      config,
      null,
      { status, statusText: "", data, headers, config },
    );
  };
}

/** Adapter, имитирующий сетевой сбой без ответа сервера. */
function networkFailureAdapter() {
  return async (config: InternalAxiosRequestConfig) => {
    throw new AxiosError("Network Error", "ERR_NETWORK", config);
  };
}

describe("createRestClient", () => {
  it("throws RestApiError with problem details from the response body", async () => {
    const client = createRestClient("https://bff.localhost/api");
    const problem = { title: "InvalidDisplayName", status: 400, detail: "Недопустимое имя" };

    const error = await client
      .get("/users/me", { adapter: failingAdapter(400, problem) })
      .catch((e: unknown) => e);

    expect(error).toBeInstanceOf(RestApiError);
    expect(error).toMatchObject({ kind: "rest", status: 400, retryable: false });
    expect((error as RestApiError).problem).toEqual(problem);
  });

  it("marks 5xx responses as retryable", async () => {
    const client = createRestClient("https://bff.localhost/api");

    const error = await client
      .get("/users/me", { adapter: failingAdapter(503, { title: "Unavailable" }) })
      .catch((e: unknown) => e);

    expect(error).toMatchObject({ status: 503, retryable: true });
  });

  it("returns RestApiError with null status when there is no response", async () => {
    const client = createRestClient("https://bff.localhost/api");

    const error = await client
      .get("/users/me", { adapter: networkFailureAdapter() })
      .catch((e: unknown) => e);

    expect(error).toBeInstanceOf(RestApiError);
    expect(error).toMatchObject({ status: null, retryable: true, problem: null });
  });

  it("keeps non-problem body as null problem", async () => {
    const client = createRestClient("https://bff.localhost/api");

    const error = await client
      .get("/users/me", { adapter: failingAdapter(502, "<html>Bad Gateway</html>") })
      .catch((e: unknown) => e);

    expect(error).toBeInstanceOf(RestApiError);
    expect((error as RestApiError).problem).toBeNull();
  });
});
