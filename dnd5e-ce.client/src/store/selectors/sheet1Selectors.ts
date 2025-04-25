import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../types/state";

export const selectSheet1 = (state: RootState) => state.sheet1;

export const selectCharacterName = (state: RootState) => state.sheet1.name;
export const selectCharacterLevel = (state: RootState) => state.sheet1.level;

export const selectAbilities = (state: RootState) => state.sheet1.abilities;

export const selectProficiencyBonus = createSelector(
  [selectCharacterLevel],
  (level) => Math.floor((level - 1) / 4) + 2
);
// ... другие селекторы