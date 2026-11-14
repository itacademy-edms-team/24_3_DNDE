import { describe, expect, it } from "vitest";

import type { EditorCard } from "./ElementCard";
import { swapCards } from "./swapCards";

function card(id: string): EditorCard {
  return {
    id,
    isDraft: false,
    type: "flashcard",
    payload: { term: id, definition: "", frontImageId: null, backImageId: null },
  };
}

describe("swapCards", () => {
  it("swaps two non-adjacent cards", () => {
    const cards = [card("a"), card("b"), card("c"), card("d")];
    expect(swapCards(cards, "a", "c").map((c) => c.id)).toEqual(["c", "b", "a", "d"]);
  });

  it("swaps adjacent cards in either direction", () => {
    const cards = [card("a"), card("b")];
    expect(swapCards(cards, "a", "b").map((c) => c.id)).toEqual(["b", "a"]);
    expect(swapCards(cards, "b", "a").map((c) => c.id)).toEqual(["b", "a"]);
  });

  it("returns the same reference when swapping a card with itself", () => {
    const cards = [card("a"), card("b")];
    expect(swapCards(cards, "a", "a")).toBe(cards);
  });

  it("returns the same reference when a card is unknown", () => {
    const cards = [card("a"), card("b")];
    expect(swapCards(cards, "a", "zzz")).toBe(cards);
  });
});
