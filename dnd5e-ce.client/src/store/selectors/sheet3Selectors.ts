import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../types/state";

import { selectAbilities, selectProficiencyBonus } from "../selectors/sheet1Selectors";

export const selectSheet3 = (state: RootState) => state.sheet3;
export const selectSpellCastingAbility = (state: RootState) => state.sheet3.spellBondAbility;

export const selectSpellCastingAbilityModifier = createSelector(
  [selectAbilities, selectSpellCastingAbility],
  (abilities, spellCastingAbility) => {
    if (spellCastingAbility === 'none') {
      return 0;
    }
    const abilityValue = abilities[spellCastingAbility].base || 10;
    return Math.floor((abilityValue - 10) / 2);
  }
);

export const selectSpellAttackBonus = createSelector(
  [
    selectSpellCastingAbility,
    selectSpellCastingAbilityModifier,
    selectProficiencyBonus
  ],
  (bondAbility, abilityModifier, proficiencyBonus) => {
    if (bondAbility === "none") {
      return 0;
    }
    return abilityModifier + proficiencyBonus;
  }
);

export const selectSpellSaveDC = createSelector(
  [
    selectSpellCastingAbility,
    selectSpellAttackBonus
  ],
  (bondAbility, spellAttackBonus) => {
    if (bondAbility === "none") {
      return 0;
    }
    return 8 + spellAttackBonus;
  }
);
