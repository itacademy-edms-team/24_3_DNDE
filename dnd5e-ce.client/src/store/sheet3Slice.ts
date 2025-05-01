
import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { Spell, CantripProgressionType, Sheet3State, SpellCategoryKey, SpellAbilityType, SpellAttackType, SpellDescriptionInAttackIncludeVariety, SpellHighLevelCastDiceType, SpellOutputType, SpellSchool, SpellLevel, RemainingSpellSlots } from '../types/state';

const initialState: Sheet3State = {
  spellBondAbility: "none",
  spells: {
    cantrips: [],
    level1: [],
    level2: [],
    level3: [],
    level4: [],
    level5: [],
    level6: [],
    level7: [],
    level8: [],
    level9: []
  },
  remainingSpellSlots: {
    level1: 0,
    level2: 0,
    level3: 0,
    level4: 0,
    level5: 0,
    level6: 0,
    level7: 0,
    level8: 0,
    level9: 0
  }
}

const sheet3Slice = createSlice({
  name: 'sheet3',
  initialState,
  reducers: {
    updateCharacterSpellCastingAbility(state, action: PayloadAction<SpellAbilityType>): void {
      state.spellBondAbility = action.payload;
    },
    addSpell: (
      state,
      action: PayloadAction<{ spell: Spell; level: SpellLevel }>
    ) => {
      const { spell, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      state.spells[key].push(spell);
    },
    deleteSpell: (
      state,
      action: PayloadAction<{ id: string; level: SpellLevel }>
    ) => {
      const { id, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      state.spells[key] = state.spells[key].filter((spell) => spell.id !== id);
    },
    updateSpellName: (
      state,
      action: PayloadAction<{ id: string; value: string; level: SpellLevel }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.name = value;
    },
    updateSpellSchool: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellSchool;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.school = value;
    },
    updateSpellIsRitual: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.isRitual = value;
    },
    updateSpellCastingTime: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.castingTime = value;
    },
    updateSpellRange: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.range = value;
    },
    updateSpellTarget: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.target = value;
    },
    updateSpellComponentsV: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.components.v.isIncluded = value;
    },
    updateSpellComponentsS: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.components.s.isIncluded = value;
    },
    updateSpellComponentsM: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.components.m.isIncluded = value;
    },
    updateSpellComponentsDescription: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.componentsDescription = value;
    },
    updateSpellIsConcentration: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.isConcentration = value;
    },
    updateSpellDuration: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.duration = value;
    },
    updateSpellCastingAbility: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellAbilityType;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.spellCastingAbility = value;
    },
    updateSpellInnate: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.innate = value;
    },
    updateSpellOutput: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellOutputType;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.output = value;
    },
    updateSpellAttackType: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellAttackType;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.attack.attackType = value;
    },
    updateSpellAttackDamage1Dice: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.attack.damage1.dice = value;
    },
    updateSpellAttackDamage1Type: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.attack.damage1.type = value;
    },
    updateSpellAttackDamage2Dice: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.attack.damage2.dice = value;
    },
    updateSpellAttackDamage2Type: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.attack.damage2.type = value;
    },
    updateSpellHealingDice: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.healingDice = value;
    },
    updateSpellIsAbilityModIncluded: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.isAbilityModIncluded = value;
    },
    updateSpellSavingThrowAbility: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellAbilityType;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.savingThrow.ability = value;
    },
    updateSpellSavingThrowEffect: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.savingThrow.effect = value;
    },
    updateSpellHigherLevelCastDiceAmount: (
      state,
      action: PayloadAction<{
        id: string;
        value: number;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.higherLevelCast.diceAmount = value;
    },
    updateSpellHigherLevelCastDiceType: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellHighLevelCastDiceType;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.higherLevelCast.diceType = value;
    },
    updateSpellHigherLevelCastBonus: (
      state,
      action: PayloadAction<{
        id: string;
        value: number;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.higherLevelCast.bonus = value;
    },
    updateSpellIncludeSpellDescriptionInAttack: (
      state,
      action: PayloadAction<{
        id: string;
        value: SpellDescriptionInAttackIncludeVariety;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.includeSpellDescriptionInAttack = value;
    },
    updateSpellDescription: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.description = value;
    },
    updateSpellAtHigherLevels: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.atHigherLevels = value;
    },
    updateSpellClass: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.class = value;
    },
    updateSpellType: (
      state,
      action: PayloadAction<{
        id: string;
        value: string;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.type = value;
    },
    updateSpellCantripProgression: (
      state,
      action: PayloadAction<{
        id: string;
        value: CantripProgressionType;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      const key: SpellCategoryKey = level === 0 ? 'cantrips' : `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.cantripProgression = value;
    },
    updateSpellIsPrepared: (
      state,
      action: PayloadAction<{
        id: string;
        value: boolean;
        level: SpellLevel;
      }>
    ) => {
      const { id, value, level } = action.payload;
      if (level === 0) return; // Cantrips don't have isPrepared
      const key: SpellCategoryKey = `level${level}`;
      const spell = state.spells[key].find((s) => s.id === id);
      if (spell) spell.isPrepared = value;
    },
    updateRemainingSpellSlots: (
      state,
      action: PayloadAction<{ level: number; value: number }>
    ) => {
      const key = `level${action.payload.level}` as RemainingSpellSlots;
      state.remainingSpellSlots[key] = Math.max(0, action.payload.value);
    },
  },
});

export const {
  updateCharacterSpellCastingAbility,
  addSpell,
  deleteSpell,
  updateSpellName,
  updateSpellSchool,
  updateSpellIsRitual,
  updateSpellCastingTime,
  updateSpellRange,
  updateSpellTarget,
  updateSpellComponentsV,
  updateSpellComponentsS,
  updateSpellComponentsM,
  updateSpellComponentsDescription,
  updateSpellIsConcentration,
  updateSpellDuration,
  updateSpellCastingAbility,
  updateSpellInnate,
  updateSpellOutput,
  updateSpellAttackType,
  updateSpellAttackDamage1Dice,
  updateSpellAttackDamage1Type,
  updateSpellAttackDamage2Dice,
  updateSpellAttackDamage2Type,
  updateSpellHealingDice,
  updateSpellIsAbilityModIncluded,
  updateSpellSavingThrowAbility,
  updateSpellSavingThrowEffect,
  updateSpellHigherLevelCastDiceAmount,
  updateSpellHigherLevelCastDiceType,
  updateSpellHigherLevelCastBonus,
  updateSpellIncludeSpellDescriptionInAttack,
  updateSpellDescription,
  updateSpellAtHigherLevels,
  updateSpellClass,
  updateSpellType,
  updateSpellCantripProgression,
  updateSpellIsPrepared,
  updateRemainingSpellSlots,
} = sheet3Slice.actions;
export default sheet3Slice.reducer;
