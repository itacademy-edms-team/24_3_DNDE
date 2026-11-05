import { beforeEach, describe, expect, it } from "vitest";

import { canRedirectToLogin, markLoginRedirect, resetAuthRedirects } from "./authRedirectGuard";

describe("authRedirectGuard", () => {
  beforeEach(() => {
    resetAuthRedirects();
  });

  it("allows the first redirect to login", () => {
    expect(canRedirectToLogin()).toBe(true);
  });

  it("blocks a second redirect while the session is still absent", () => {
    markLoginRedirect();

    expect(canRedirectToLogin()).toBe(false);
  });

  it("allows redirects again after a successful session", () => {
    markLoginRedirect();
    resetAuthRedirects();

    expect(canRedirectToLogin()).toBe(true);
  });
});
