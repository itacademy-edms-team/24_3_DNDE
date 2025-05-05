import { RootState } from "../../types/state";

export const selectSheet2 = (state: RootState) => state.sheet2;
export const selectAge = (state: RootState): string => state.sheet2.age;
export const selectHeight = (state: RootState): string => state.sheet2.height;
export const selectWeight = (state: RootState): string => state.sheet2.weight;
export const selectEyes = (state: RootState): string => state.sheet2.eyes;
export const selectSkin = (state: RootState): string => state.sheet2.skin;
export const selectHair = (state: RootState): string => state.sheet2.hair;

export const selectAppearance = (state: RootState): string => state.sheet2.appearance;
export const selectBackstory = (state: RootState): string => state.sheet2.backstory;
export const selectAlliesAndOrganizations = (state: RootState): string => state.sheet2.alliesAndOrganizations;
export const selectAdditionalFeaturesAndTraits = (state: RootState): string => state.sheet2.additionalFeaturesAndTraits;
export const selectTreasures = (state: RootState): string => state.sheet2.treasures;
// ... другие селекторы