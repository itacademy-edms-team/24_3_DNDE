import { createSelector } from "@reduxjs/toolkit";
import { ClassInfo, PactMagic, RootState, SpellSlots } from "../../types/state";

export const selectSheet1 = (state: RootState) => state.sheet1;

export const selectCharacterName = (state: RootState) => state.sheet1.name;
export const selectCharacterClass = (state: RootState) => state.sheet1.class;
export const selectCharacterLevel = (state: RootState) => state.sheet1.level;
export const selectCharacterRace = (state: RootState) => state.sheet1.race;
export const selectCharacterBackstory = (state: RootState) => state.sheet1.backstory;
export const selectCharacterWorldview = (state: RootState) => state.sheet1.worldview;
export const selectCharacterPlayerName = (state: RootState) => state.sheet1.playerName;
export const selectCharacterExperience = (state: RootState) => state.sheet1.experience;

export const selectAbilities = (state: RootState) => state.sheet1.abilities;

export const selectProficiencyBonus = createSelector(
  [selectCharacterLevel],
  (level) => Math.floor((level - 1) / 4) + 2
);
// ... другие селекторы

export const selectClassResource = (state: RootState) => state.sheet1.classResource;
export const selectOtherResources = (state: RootState) => state.sheet1.otherResources;
