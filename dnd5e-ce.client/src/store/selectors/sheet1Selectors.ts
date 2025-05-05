import { createSelector } from "@reduxjs/toolkit";
import { RootState, AbilityType, SkillType } from "../../types/state";
import { calcPbMultiplier } from "../../utils/calc";

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
export const selectIsInspired = (state: RootState) => state.sheet1.isInspired

export const selectProficiencyBonus = createSelector(
  [selectCharacterLevel],
  (level) => Math.floor((level - 1) / 4) + 2
);
// ... другие селекторы

export const selectAbilityScoreModifiers = createSelector(
  [selectAbilities],
  (abilities) => {
    return {
      strength: Math.floor((abilities.strength.base - 10) / 2),
      dexterity: Math.floor((abilities.dexterity.base - 10) / 2),
      constitution: Math.floor((abilities.constitution.base - 10) / 2),
      intelligence: Math.floor((abilities.intelligence.base - 10) / 2),
      wisdom: Math.floor((abilities.wisdom.base - 10) / 2),
      charisma: Math.floor((abilities.charisma.base - 10) / 2)
    }
  }
);

export const selectAbilityModifier = createSelector(
  [
    selectAbilityScoreModifiers,
    (_: RootState, ability: AbilityType) => ability,
  ],
  (modifiers, ability): number => {
    return modifiers[ability] ?? 0;
  }
);

export const selectSaveThrows = (state: RootState) => state.sheet1.saveThrows;
export const selectSaveThrowValues = createSelector(
  [selectSaveThrows, selectAbilityScoreModifiers, selectProficiencyBonus],
  (st, am, pb) => {
    return {
      strength: st.strength.isProficient ? am.strength + pb : am.strength,
      dexterity: st.dexterity.isProficient ? am.dexterity + pb : am.dexterity,
      constitution: st.constitution.isProficient ? am.constitution + pb : am.constitution,
      intelligence: st.intelligence.isProficient ? am.intelligence + pb : am.intelligence,
      wisdom: st.wisdom.isProficient ? am.wisdom + pb : am.wisdom,
      charisma: st.charisma.isProficient ? am.charisma + pb : am.charisma
    }
  }
);

export const selectSkills = (state: RootState) => state.sheet1.skills;
export const selectSkillValues = createSelector(
  [selectSkills, selectAbilityScoreModifiers, selectProficiencyBonus],
  (skills, am, pb) => {
    const skillValues = {} as Record<SkillType, number>;
    for (const skill in skills) {
      const isProficient = skills[skill as SkillType].isProficient;
      skillValues[skill as SkillType] = isProficient
        ? am[skills[skill as SkillType].ability] + pb
        : am[skills[skill as SkillType].ability];
    }
    return skillValues;
  }
);

export const selectPassivePerception = createSelector(
  [selectSkills, selectAbilityScoreModifiers, selectProficiencyBonus],
  (skills, am, pb) => {
    const isProficient = skills.perception.isProficient;
    return isProficient ? 10 + am.wisdom + pb : 10 + am.wisdom;
  }
);

export const selectTools = (state: RootState) => state.sheet1.tools;
export const selectToolCalculatedProficiencies = createSelector(
  [selectTools, selectAbilityScoreModifiers, selectProficiencyBonus],
  (tools, am, pb) => tools.map(tool => {
    return {
      toolId: tool.id,
      result: am[tool.bondAbility] + pb * calcPbMultiplier(tool.proficiencyType) + tool.mods
    }
  })
);

export const selectOtherTools = (state: RootState) => state.sheet1.otherTools;

export const selectArmorClass = (state: RootState) => state.sheet1.armorClass;
export const selectInitiative = (state: RootState) => state.sheet1.initiative;
export const selectCharacterSpeed = (state: RootState) => state.sheet1.speed;
export const selectCurrentHP = (state: RootState) => state.sheet1.hp.current;
export const selectMaxHP = (state: RootState) => state.sheet1.hp.max;
export const selectTempHP = (state: RootState) => state.sheet1.hp.temp;
export const selectHitDiceTotal = (state: RootState) => state.sheet1.hitDice.total;
export const selectHitDiceCurrent = (state: RootState) => state.sheet1.hitDice.current;
export const selectHitDiceType = (state: RootState) => state.sheet1.hitDice.type;
export const selectDeathSaveThrowsSuccesses = (state: RootState) => state.sheet1.deathSaveThrow.successes;
export const selectDeathSaveThrowsFailures = (state: RootState) => state.sheet1.deathSaveThrow.failures;
export const selectAttacks = (state: RootState) => state.sheet1.attacks;
export const selectGlobalDamageModifiers = (state: RootState) => state.sheet1.globalDamageModifiers;
export const selectInventoryGold = (state: RootState) => state.sheet1.inventory.gold;
export const selectInventoryItems = (state: RootState) => state.sheet1.inventory.items;

export const selectInventoryItemOtherResourceBonds = (state: RootState) => state.sheet1.itemOtherResourceBonds;

export const selectCharacterPersonalityTraits = (state: RootState) => state.sheet1.personalityTraits;
export const selectCharacterIdeals = (state: RootState) => state.sheet1.ideals;
export const selectCharacterBonds = (state: RootState) => state.sheet1.bonds;
export const selectCharacterFlaws = (state: RootState) => state.sheet1.flaws;
export const selectClassResource = (state: RootState) => state.sheet1.classResource;
export const selectOtherResources = (state: RootState) => state.sheet1.otherResources;

