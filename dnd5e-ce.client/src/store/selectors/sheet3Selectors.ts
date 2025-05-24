import { createSelector } from "@reduxjs/toolkit";
import { ClassInfo, PactMagic, RootState, SpellCategoryKey, SpellLevel, SpellSlots } from "../../types/state";

import { selectAbilities, selectCharacterClass, selectCharacterLevel, selectProficiencyBonus } from "../selectors/sheet1Selectors";

export const selectSheet3 = (state: RootState) => state.sheet3;
export const selectSpellCastingAbility = (state: RootState) => state.sheet3.spellBondAbility;

export const selectSpellCastingAbilityModifier = createSelector(
  [selectAbilities, selectSpellCastingAbility],
  (abilities, spellCastingAbility) => {
    if (spellCastingAbility === 'none') {
      return 0;
    }
    const abilityValue = abilities[spellCastingAbility]?.base || 10;
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

export const selectSpells = (state: RootState) => state.sheet3.spells;
export const selectCantrips = (state: RootState) => state.sheet3.spells.cantrips;
export const selectSpellsByLevel = (level: SpellLevel) => (state: RootState) => {
  const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
  return state.sheet3.spells[key];
};

// Spell slot tables for each class (based on D&D 5e PHB)
const spellSlotsByClass: Record<string, Record<number, SpellSlots>> = {
  wizard: {
    1: { 1: 2 },
    2: { 1: 3 },
    3: { 1: 4, 2: 2 },
    4: { 1: 4, 2: 3 },
    5: { 1: 4, 2: 3, 3: 2 },
    6: { 1: 4, 2: 3, 3: 3 },
    7: { 1: 4, 2: 3, 3: 3, 4: 1 },
    8: { 1: 4, 2: 3, 3: 3, 4: 2 },
    9: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 1 },
    10: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2 },
    11: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1 },
    12: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1 },
    13: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1 },
    14: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1 },
    15: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1, 8: 1 },
    16: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1, 8: 1 },
    17: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1, 8: 1, 9: 1 },
    18: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 3, 6: 1, 7: 1, 8: 1, 9: 1 },
    19: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 3, 6: 2, 7: 1, 8: 1, 9: 1 },
    20: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 3, 6: 2, 7: 2, 8: 1, 9: 1 },
  },
  // Same table for cleric, druid, sorcerer, bard (full casters)
  cleric: { /* Same as wizard */ },
  druid: { /* Same as wizard */ },
  sorcerer: { /* Same as wizard */ },
  bard: { /* Same as wizard */ },
  paladin: {
    2: { 1: 2 },
    3: { 1: 3 },
    4: { 1: 3 },
    5: { 1: 4, 2: 2 },
    6: { 1: 4, 2: 2 },
    7: { 1: 4, 2: 3 },
    8: { 1: 4, 2: 3 },
    9: { 1: 4, 2: 3, 3: 2 },
    10: { 1: 4, 2: 3, 3: 2 },
    11: { 1: 4, 2: 3, 3: 3 },
    12: { 1: 4, 2: 3, 3: 3 },
    13: { 1: 4, 2: 3, 3: 3, 4: 1 },
    14: { 1: 4, 2: 3, 3: 3, 4: 1 },
    15: { 1: 4, 2: 3, 3: 3, 4: 2 },
    16: { 1: 4, 2: 3, 3: 3, 4: 2 },
    17: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 1 },
    18: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 1 },
    19: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2 },
    20: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2 },
  },
  ranger: { /* Same as paladin */ },
  'eldritch knight': {
    3: { 1: 2 },
    4: { 1: 3 },
    7: { 1: 4, 2: 2 },
    8: { 1: 4, 2: 2 },
    10: { 1: 4, 2: 3 },
    11: { 1: 4, 2: 3 },
    13: { 1: 4, 2: 3, 3: 2 },
    14: { 1: 4, 2: 3, 3: 2 },
    16: { 1: 4, 2: 3, 3: 3 },
    19: { 1: 4, 2: 3, 3: 3, 4: 1 },
    20: { 1: 4, 2: 3, 3: 3, 4: 1 },
  },
  'arcane trickster': { /* Same as eldritch knight */ },
};

// Warlock Pact Magic table
const pactMagicByLevel: Record<number, PactMagic> = {
  1: { slots: 1, level: 1 },
  2: { slots: 2, level: 1 },
  3: { slots: 2, level: 2 },
  4: { slots: 2, level: 2 },
  5: { slots: 2, level: 3 },
  6: { slots: 2, level: 3 },
  7: { slots: 2, level: 4 },
  8: { slots: 2, level: 4 },
  9: { slots: 2, level: 5 },
  10: { slots: 2, level: 5 },
  11: { slots: 3, level: 5 },
  12: { slots: 3, level: 5 },
  13: { slots: 3, level: 5 },
  14: { slots: 3, level: 5 },
  15: { slots: 3, level: 5 },
  16: { slots: 3, level: 5 },
  17: { slots: 4, level: 5 },
  18: { slots: 4, level: 5 },
  19: { slots: 4, level: 5 },
  20: { slots: 4, level: 5 },
};

// Multiclass spell slot table (PHB p. 165)
const multiclassSpellSlots: Record<number, SpellSlots> = {
  1: { 1: 2 },
  2: { 1: 3 },
  3: { 1: 4, 2: 2 },
  4: { 1: 4, 2: 3 },
  5: { 1: 4, 2: 3, 3: 2 },
  6: { 1: 4, 2: 3, 3: 3 },
  7: { 1: 4, 2: 3, 3: 3, 4: 1 },
  8: { 1: 4, 2: 3, 3: 3, 4: 2 },
  9: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 1 },
  10: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2 },
  11: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1 },
  12: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1 },
  13: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1 },
  14: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1 },
  15: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1, 8: 1 },
  16: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1, 8: 1 },
  17: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 2, 6: 1, 7: 1, 8: 1, 9: 1 },
  18: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 3, 6: 1, 7: 1, 8: 1, 9: 1 },
  19: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 3, 6: 2, 7: 1, 8: 1, 9: 1 },
  20: { 1: 4, 2: 3, 3: 3, 4: 3, 5: 3, 6: 2, 7: 2, 8: 1, 9: 1 },
};

// Selector to calculate caster level for multiclassing
export const selectCasterLevel = createSelector(
  [selectCharacterClass, selectCharacterLevel],
  (characterClass, totalLevel) => {
    // Handle single class (string) or multiclass (array of ClassInfo)
    const classes: ClassInfo[] = typeof characterClass === 'string'
      ? [{ class: characterClass, level: totalLevel }]
      : characterClass;

    return classes.reduce((total, { class: className, level }) => {
      if (['wizard', 'cleric', 'druid', 'sorcerer', 'bard'].includes(className)) {
        return total + level;
      } else if (['paladin', 'ranger'].includes(className)) {
        return total + Math.floor(level / 2);
      } else if (['eldritch knight', 'arcane trickster'].includes(className)) {
        return total + Math.floor(level / 3);
      }
      return total; // Warlock levels don't contribute to caster level
    }, 0);
  }
);

// Selector for standard spell slots
export const selectSpellSlots = createSelector(
  [selectCharacterClass, selectCharacterLevel, selectCasterLevel],
  (characterClass, totalLevel, casterLevel) => {
    // Handle single class
    if (typeof characterClass === 'string' && characterClass !== 'warlock') {
      return spellSlotsByClass[characterClass]?.[totalLevel] || {};
    }

    // Handle multiclass (including warlock as a special case)
    return multiclassSpellSlots[casterLevel] || {};
  }
);

// Selector for warlock Pact Magic slots
export const selectPactMagic = createSelector(
  [selectCharacterClass, selectCharacterLevel],
  (characterClass, totalLevel) => {
    // Handle single class
    if (typeof characterClass === 'string' && characterClass === 'warlock') {
      return pactMagicByLevel[totalLevel] || { slots: 0, level: 0 };
    }

    // Handle multiclass
    if (Array.isArray(characterClass)) {
      const warlockClass = characterClass.find((c) => c.class === 'warlock');
      if (warlockClass) {
        return pactMagicByLevel[warlockClass.level] || { slots: 0, level: 0 };
      }
    }

    return null; // No Pact Magic if not a warlock
  }
);

export const selectRemainingSpellSlots = (state: RootState) => state.sheet3.remainingSpellSlots;