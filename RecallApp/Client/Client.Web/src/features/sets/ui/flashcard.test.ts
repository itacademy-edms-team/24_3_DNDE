import { describe, expect, it } from "vitest";

import { flashcard } from "./flashcard";

describe("flashcard", () => {
  it("maps payload + images into a draft with side images", () => {
    const draft = flashcard.toDraft(
      { term: "t", definition: "d", frontImageId: "m1", backImageId: null },
      {
        front: {
          thumb: { url: "http://thumb1", width: 100, height: 100 },
          display: { url: "http://display1", width: 400, height: 400 },
        },
      },
    );

    expect(draft.frontImage).toEqual({
      mediaId: "m1",
      thumbUrl: "http://thumb1",
    });
    expect(draft.backImage).toBeNull();
  });

  it("serializes draft side images back into payload ids", () => {
    const payload = flashcard.toApiPayload({
      term: "t",
      definition: "d",
      frontImage: { mediaId: "m1", thumbUrl: "http://thumb1" },
      backImage: null,
    });

    expect(payload).toEqual({
      term: "t",
      definition: "d",
      frontImageId: "m1",
      backImageId: null,
    });
  });

  it("keeps the side image null when there is no id or no thumb", () => {
    const draft = flashcard.toDraft(
      { term: "t", definition: "d", frontImageId: "m1", backImageId: null },
      null,
    );

    expect(draft.frontImage).toBeNull();
  });
});
